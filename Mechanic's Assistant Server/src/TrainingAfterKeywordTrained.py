from ClusteringComplaintTraining import train as clusterTrain
from KNNTraining import train as knnTrain

#assume datafile is in ../Data/trainDat3.txt
#assume bayesfile is in KeywordPrediction/outmod.nbmdl

datafile = "../Data/trainDat3.txt"
bayesfile = "KeywordPrediction/outmod.nbmdl"
complaintclusterfile = "Clustering/ComplaintKeywordClusteringModeltest.kcgmdl"
knnfile = "KNNPrediction/KNNProblemPredictionModel.knnmdl"

clusterTrain(datafile, bayesfile, complaintclusterfile)
knnTrain(datafile, bayesfile, complaintclusterfile, knnfile)