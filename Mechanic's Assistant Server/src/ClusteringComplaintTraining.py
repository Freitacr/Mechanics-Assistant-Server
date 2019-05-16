from Clustering.KeywordClustering import Group, Cluster
from KeywordPrediction import KeywordBayes
from Util.JimDataLoader import openFile
import sys
import nltk as n

#trains model
def train(datafilename, bayes_model_filename, out_model_filename):
	data = openFile(datafilename)
	sentences = []
	for training_case in data:
		sentences.extend([training_case[3].split(":")[1].lstrip().rstrip()])
	model = KeywordBayes.load_model(bayes_model_filename)
	keyword_data = []
	for sentence in sentences:
		keywords = KeywordBayes.predict(model, sentence)
		for keyword_index in range(len(keywords)):
			keywords[keyword_index] = keywords[keyword_index].lower()
		keyword_data.extend([keywords])
	clusterer = Cluster()
	clusterer.train(keyword_data)
	clusterer.store(out_model_filename)

if __name__ == "__main__":
	datafile, bayes_model_filename, out_model_filename = sys.argv[1:]
	train(datafile, bayes_model_filename, out_model_filename)