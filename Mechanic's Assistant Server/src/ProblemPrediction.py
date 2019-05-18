from src.KNNPrediction.KNN import KNN
from src.Clustering.KeywordClustering import Cluster
from src.KeywordPrediction import KeywordBayes
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
    ret = ArgumentParser()
    ret.add_argument("-I", required=False, dest="I", type=bool, default=False)
    ret.add_argument("-f", required=False, dest="f", type=str, default = "")
    namespace = ret.parse_args()
    if (namespace.f == "" and not namespace.I):
        print("program must be run either in Interactive mode, or with a file as an input")
        ret.print_usage()
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
    return ret ** (1/2)

if __name__ == "__main__":
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

    knn = KNN()
    knn.load(knnfile)

    complaint_cluster = Cluster()
    complaint_cluster.load(complaintclusterfile)

    keywordBayes = KeywordBayes.load_model(bayesfile)

    if (namespace.I):
        usrinput = input("Please enter the make, model, and customer complaint, or type exit() to exit\n")
        while not usrinput == "exit()":
            inputsplit = usrinput.lstrip().rstrip().split(" ")
            make = inputsplit[0]
            model = inputsplit[1]
            if (model == ""):
                usrinput = input("I'm sorry, something wasn't quite right about that\nToo many spaces in between words perhaps?\nPlease try retyping it\n")
                continue
            complaint = inputsplit[2:]
            complaint_string = ""
            for part in complaint:
                complaint_string += part + " "
            complaint_string.rstrip()
            keywords = KeywordBayes.predict(keywordBayes, complaint_string)
            groups = complaint_cluster.predict_top_n(keywords, 3)
            example = [make, model]
            example.extend(groups)
            ret = knn.predict(example, 10, 2, distanceCalc)
            print("\n<-------------------------->")
            for x in ret:
                print(x[0][1])
            print("<-------------------------->")
            print("\n\n")
            usrinput = input("Please enter the make, model, and customer complaint, or type exit() to exit\n")
    else:
        dataFile = None
        try:
            dataFile = open(namespace.f, "r")
        except FileNotFoundError:
            print(namespace.f, "was not found, please verify that the file exists")
        data = json.load(dataFile)
        dataFile.close();
        necessaryComponents = ["Make", "Model", "Complaint"]
        for component in necessaryComponents:
            if not component in data:
                print("Improper format for file", namespace.f)
                exit(4)
        make = data[necessaryComponents[0]]
        model = data[necessaryComponents[1]]
        complaint = data[necessaryComponents[2]]
        keywords = KeywordBayes.predict(keywordBayes, complaint)
        groups = complaint_cluster.predict_top_n(keywords, 3)
        example = [make, model]
        example.extend(groups)
        results = knn.predict(example, 10, 2, distanceCalc)
        returnObject = []
        for resultVal in results:
            returnObject.append({"Problem":resultVal[0][1][0]})
        printFormatter = open(namespace.f, "w")
        json.dump(returnObject, printFormatter)
        printFormatter.close()
        