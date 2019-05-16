from Util.JimDataLoader import openFile
from KeywordPrediction import KeywordBayes, KeywordBayesTrainingUtils as KBTU
import sys
import os
import nltk as n

filename, modelfilename = sys.argv[1:]
dataFile = None
try:
	dataFile = open(filename)
	dataFile.close()
except FileNotFoundError:
	print("File", filename, "not found, exiting")
	sys.exit(1)
	

data = openFile(filename)
sentences = []
for training_case in data:
	sentences.extend([training_case[3].split(":")[1].lstrip().rstrip()])
	sentences.extend([training_case[4].split(":")[1].lstrip().rstrip()])
X, Y = [[], []]
for sentence in sentences:
	examples, value = KBTU.setup_training_examples_for_sentence(KBTU.split_input_into_examples(n.pos_tag(n.word_tokenize(sentence))), sentence)
	X.extend(examples)
	Y.extend(value)
KBTU.store_examples("temp.dat", X, Y)
KeywordBayes.train_model("KeywordPrediction/" + modelfilename, "temp.dat")
os.remove("temp.dat")