from KNNPrediction.KNN import KNN
from Clustering.KeywordClustering import Cluster
from KeywordPrediction import KeywordBayes
import sys

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

#TODO



datafile = "../Data/trainDat3.txt"
knnfile = "KNNPrediction/KNNProblemPredictionModel.knnmdl"
bayesfile = "KeywordPrediction/outmod.nbmdl"
complaintclusterfile = "Clustering/ComplaintKeywordClusteringModeltest.kcgmdl"


try:
	open(knnfile, 'r')
except FileNotFoundError:
	print("File", knnfile, "was not found, attempting retraining")
	import TrainingAfterKeywordTrained
	print("retraining successful, continuing without error")

#can almost safely now assume bayes model and clustering model exist
additive_mode = False
added_data = []
curr_data = []

if len(sys.argv) == 2:
	if int(sys.argv[1]) == 0:
		additive_mode = True

if additive_mode:
	file = open(datafile, 'r')
	for line in file:
		curr_data.extend([line])
	file.close()

knn = KNN()
knn.load(knnfile)

complaint_cluster = Cluster()
complaint_cluster.load(complaintclusterfile)

keywordBayes = KeywordBayes.load_model(bayesfile)


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
	
	if additive_mode:
		keep = input("Would you like to add this data to the database?\n")
		if keep.lower() in ['y', 'yes', 'yeah', 'sure', 'ok', 'why not']:
			problem = input("Then please enter what the real problem was:\n")
			vin = input("Thank you, now please enter the VIN:\n")
			datastring = make + "\t"
			datastring += model + "\t"
			datastring += vin + "\t"
			datastring += "COMPLAINT:" + complaint_string + "\t"
			datastring += "PRIMARY CAUSE:" + problem + "\n"
			added_data.extend([datastring])
		
	
	
	
	
	usrinput = input("Please enter the make, model, and customer complaint, or type exit() to exit\n")

curr_data.extend(added_data)
file = open("newTrainDat.txt", 'w')
for data in curr_data:
	file.write(data)
file.close()