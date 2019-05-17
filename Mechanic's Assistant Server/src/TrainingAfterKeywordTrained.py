from src.ClusteringComplaintTraining import train as clusterTrain
from src.KNNTraining import train as knnTrain

#assume datafile is in ../Data/trainDat3.txt
#assume bayesfile is in KeywordPrediction/outmod.nbmdl

datafile = "Data/trainDat3.txt"
bayesfile = "src/KeywordPrediction/outmod.nbmdl"
complaintclusterfile = "src/Clustering/ComplaintKeywordClusteringModeltest.kcgmdl"
knnfile = "src/KNNPrediction/KNNProblemPredictionModel.knnmdl"

clusterTrain(datafile, bayesfile, complaintclusterfile)
knnTrain(datafile, bayesfile, complaintclusterfile, knnfile)