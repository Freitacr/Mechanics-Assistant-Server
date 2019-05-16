from KeywordPrediction import KeywordBayes
from Util.JimDataLoader import openFile
from Clustering.KeywordClustering import Cluster
from KNNPrediction.KNN import KNN
from random import randint

import sys
import nltk as n

#Custom distance calculation 
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

#remove the no values
def remove_nones(list):
	temp = []
	for x in list:
		if not x == None:
			temp.extend([x])
	return temp

#splits data into two groups one test and one training
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
		#for y in range(amount_per_fold):
		#	selection_index = randint(0, len(X) - 1)
		#	while not selection_index in rand_pool:
		#		selection_index = randint(0, len(X)-1)
		#	selection_indice_list.extend([selection_index])
		#	for index in range(len(rand_pool)):
		#		if rand_pool[index] == selection_index:
		#			rand_pool[index] = None
		#			break
		#rand_pool = remove_nones(rand_pool)
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

#grabs random index check if used or not. if not use it now			
def getNumRandSamples(amount, pool, sample_index_list, X):
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
	
#trains model TODO
def train(datafile, bayes_model_filename, cluster_complaint_model_filename, out_model_filename):
	try:
		open(datafile)
	except FileNotFoundError:
		print("File", datafile, "was not found, exiting")
		sys.exit(1)
	data = openFile(datafile)
	complaint_sentences = []
	newDataExamples = []
	problem_sentences = []
	for training_case in data:
		newDataExamples.extend([training_case[:2]])
		complaint_sentences.extend([training_case[3].split(":")[1].lstrip().rstrip()])
		problem_sentences.extend([[training_case[4].split(":")[1].lstrip().rstrip().capitalize()]])
	bayes_model = KeywordBayes.load_model(bayes_model_filename)
	cluster_compalint_model = Cluster()
	cluster_compalint_model.load(cluster_complaint_model_filename)
	X,Y = [[],[]]
	for index in range(len(complaint_sentences)):
		keywords = KeywordBayes.predict(bayes_model, complaint_sentences[index])
		groups = cluster_compalint_model.predict_top_n(keywords, 3)
		newDataExamples[index].extend(groups)
		X.extend([newDataExamples[index]])
		Y.extend([problem_sentences[index]])

	knn = KNN()







	knn.train(X[:-10],Y[:-10])
	pred_amount = X[-10:-1]
	pred_labels = Y[-10:-1]
	knn.store(out_model_filename)
	#cross_fold_training(X, Y, knn, 10)


if __name__ == "__main__":
	datafile, bayes_model_filename, cluster_complaint_model_filename, out_model_filename = sys.argv[1:]
	train(datafile, bayes_model_filename, cluster_complaint_model_filename, out_model_filename)

#for example_index in range(len(pred_amount)):
#	print("Example", pred_amount[example_index], pred_labels[example_index])
#	print("Example Transformed", knn.transform_example_volitile(pred_amount[example_index]), pred_labels[example_index])
#	ret = knn.predict(pred_amount[example_index], 10, 2, distanceCalc)
#	for x in ret:
#		print(x)
#	print("\n")
#print("---------------------------------------------\n")

