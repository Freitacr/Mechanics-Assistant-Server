from src.KeywordPrediction import KeywordBayes
from src.Util.JimDataLoader import openFile
from src.Clustering.KeywordClustering import Cluster
from src.KNNPrediction.KNN import KNN
from random import randint
from typing import List

import sys
import nltk as n

def distanceCalc(x : List[float], y : List[float]) -> float:
    '''Custom distance calculation method for two points, x and y'''
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

def remove_nones(list):
    '''Removes all values from the provided list that are the value None'''
    temp = []
    for x in list:
        if not x == None:
            temp.extend([x])
    return temp

#splits data into two groups one test and one training
# Unused...
def cross_fold_training(X, Y, knn, num_of_folds, results_per_example = 10):
    amount_per_fold = int(len(X) / num_of_folds)
    rand_pool = []
    rand_pool.extend(range(len(X)))
    selection_indice_list = []
    used_list = []
    for x in range(num_of_folds):
        print("New Fold\n")
        training_listX = []
        training_listX.extend(X)
        training_listY = []
        training_listY.extend(Y)
        getNumRandSamples(amount_per_fold, rand_pool, selection_indice_list, X)
        pred_list = []
        pred_label = []
        
        for index in selection_indice_list:
            pred_list.extend([training_listX[index]])
            pred_label.extend([training_listY[index]])
            training_listX[index] = None
            training_listY[index] = None
        training_listX = remove_nones(training_listX)
        training_listY = remove_nones(training_listY)
        knn = KNN()
        knn.train(training_listX, training_listY)
        
        for x in range(len(pred_list)):
            print("Example", pred_list[x], pred_label[x])
            print("Example Transformed", knn.transform_example_volitile(pred_list[x]), pred_label[x])
            ret = knn.predict(pred_list[x], results_per_example, 2, distanceCalc)
            for y in ret:
                print(y)
            print("\n")
        
        print("\n\n")
        
        
        selection_indice_list = []
    
    if not len(rand_pool) == 0:
        training_listX = []
        training_listX.extend(X)
        training_listY = []
        training_listY.extend(Y)
        selection_indice_list = remove_nones(rand_pool)
        pred_list = []
        pred_label = []
        
        for index in selection_indice_list:
            pred_list.extend([training_listX[index]])
            pred_label.extend([training_listY[index]])
            training_listX[index] = None
            training_listY[index] = None
        training_listX = remove_nones(training_listX)
        training_listY = remove_nones(training_listY)
        knn = KNN()
        knn.train(training_listX, training_listY)
        
        for x in range(len(pred_list)):
            print("Example", pred_list[x], pred_label[x])
            print("Example Transformed", knn.transform_example_volitile(pred_list[x]), pred_label[x])
            ret = knn.predict(pred_list[x], results_per_example, 2, distanceCalc)
            for y in ret:
                print(y)
            print("\n")


def getNumRandSamples(amount, pool, sample_index_list, X):
    '''Generates a list of unique indices to use for training'''
    for x in range(amount):
        selection_index = randint(0, len(X) -1)
        while not selection_index in pool:
            selection_index = randint(0, len(X)-1)
        sample_index_list.extend([selection_index])
        for index in range(len(pool)):
                if pool[index] == selection_index:
                    pool[index] = None
                    break
    pool = remove_nones(pool)
    
    
def train(datafile, bayes_model_filename, cluster_complaint_model_filename, out_model_filename):
    '''Loads data from dataFile, and trains a KNN model, which is stored in the file specified
    @param: bayes_model_filename: filePath to reach the stored KeywordBayes model to use for KNN training
    @param: cluster_complaint_model_filename: file path to reach the stored KeywordClustering model to use for KNN training
    @param: out_model_filename: file path to store the newly trained KNN model in'''
    try:
        file = open(datafile)
        file.close()
    except FileNotFoundError:
        print("File", datafile, "was not found, exiting")
        sys.exit(1)
    data = openFile(datafile)
    complaint_sentences = []
    newDataExamples = []
    problem_sentences = []
    #Translate loaded data into three lists, complaints, problems, and a list containing lists of makes and models
    for training_case in data:    
        newDataExamples.append([training_case["make"], training_case["model"]])
        complaint_sentences.extend([training_case["complaint"].split(":")[1].lstrip().rstrip()])
        problem_sentences.extend([[training_case["problem"].split(":")[1].lstrip().rstrip().capitalize()]])
    
    #Initialize Models to transform the data into usable values
    bayes_model = KeywordBayes.load_model(bayes_model_filename)
    cluster_compalint_model = Cluster()
    cluster_compalint_model.load(cluster_complaint_model_filename)
    X,Y = [[],[]]
    #Generate list of training data
    for index in range(len(complaint_sentences)):
        keywords = KeywordBayes.predict(bayes_model, complaint_sentences[index])
        groups = cluster_compalint_model.predict_top_n(keywords, 3)
        newDataExamples[index].extend(groups)
        X.extend([newDataExamples[index]])
        Y.extend([problem_sentences[index]])

    knn = KNN()
    knn.train(X,Y)
    knn.store(out_model_filename)


if __name__ == "__main__":
    datafile, bayes_model_filename, cluster_complaint_model_filename, out_model_filename = sys.argv[1:]
    train(datafile, bayes_model_filename, cluster_complaint_model_filename, out_model_filename)

