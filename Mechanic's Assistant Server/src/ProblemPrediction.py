from src.KNNPrediction.KNN import KNN
from src.Clustering.KeywordClustering import Cluster
from src.KeywordPrediction.KeywordBayes import NaiveBayesKeywordPredictor
from src.Clustering.ClusteringComplaintTraining import train as clusterTrain
from src.KNNPrediction.KNNTraining import train as knnTrain
import sys
from io import StringIO
import json
from argparse import ArgumentParser

datafile = "Data/trainDat3.txt"
knnfile = "Models/KNNProblemPredictionModel.knnmdl"
bayesfile = "Models/KeywordModel.nbmdl"
complaintclusterfile = "Models/ComplaintKeywordClusteringModel.kcgmdl"


def parseArgs():
    '''Parses the arguments passed into the file and returns them in a namespace object'''
    ret = ArgumentParser()
    ret.add_argument("-f", required=True, dest="f", type=str, default = "")
    namespace = ret.parse_args()
    return namespace


#custom distance formula (same point)
def distanceCalc(x, y):
    if not len(x) == len(y):
        raise ValueError("Lengths of inputs must be identical for distance calculation")
    ret = 0.0
    for axis_index in range(len(x)):
        if axis_index in range(2):
            if not y[axis_index] == x[axis_index]:
                ret += 1
                continue
            else:
                continue
        ret += (y[axis_index] - x[axis_index]) ** 2
    return ret ** (1 / 2)

def predictProblem(make, model, complaint, keywordBayes, knn) -> list:
    '''Predicts the top 10 most likely problems given the make, model, complaint using the two machine learning models passed in'''
    keywords = keywordBayes.predict(complaint)
    groups = complaint_cluster.predict_top_n(keywords, 3)
    example = [make, model]
    example.extend(groups)
    results = knn.predict(example, 10, 2, distanceCalc)
    return results

def writeResultsToFile(filePath : str, results : list) -> None:
    '''Writes the results from the problem prediction to the file specified by filePath'''
    returnObject = []
    for resultVal in results:
        returnObject.append({"Problem":resultVal[0][1][0]})
    printFormatter = open(filePath, "w")
    json.dump(returnObject, printFormatter)
    printFormatter.close()

def loadData(dataFilePath) -> list:
    '''Loads data from the file specified by dataFilePath and returns the make, model, and customer's complaint'''
    dataFile = None
    try:
        dataFile = open(dataFilePath, "r")
    except FileNotFoundError:
        print(namespace.f, "was not found, please verify that the file exists")
    data = json.load(dataFile)
    dataFile.close()
    necessaryComponents = ["Make", "Model", "Complaint"]
    for component in necessaryComponents:
        if not component in data:
            print("Improper format for file", namespace.f)
            exit(4)
    make = data[necessaryComponents[0]]
    model = data[necessaryComponents[1]]
    complaint = data[necessaryComponents[2]]
    return make, model, complaint

if __name__ == "__main__":
    #Test whether the knn model file exists, if not, then attempt retraining
    try:
        testFile = open(knnfile, 'r')
        testFile.close()
    except FileNotFoundError:
        print("File", knnfile, "was not found, attempting retraining")
        clusterTrain(datafile, bayesfile, complaintclusterfile)
        knnTrain(datafile, bayesfile, complaintclusterfile, knnfile)
        print("retraining successful, continuing without error")

    #can almost safely now assume bayes model and clustering model exists
    namespace = parseArgs()

    #arguments are valid, so load machine learning models
    knn = KNN()
    knn.load(knnfile)
    complaint_cluster = Cluster()
    complaint_cluster.load(complaintclusterfile)
    keywordBayes = NaiveBayesKeywordPredictor()
    keywordBayes.loadModel(bayesfile)
    
    #We are assuming that we need to read data from a file, so let's do that.
    make, model, complaint = loadData(namespace.f)
    results = predictProblem(make, model, complaint, keywordBayes, knn)
    writeResultsToFile(namespace.f, results)
