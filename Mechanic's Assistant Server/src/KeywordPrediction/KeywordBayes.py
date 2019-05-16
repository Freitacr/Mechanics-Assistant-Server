import KeywordPrediction.KeywordBayesTrainingUtils as KBTU
from KeywordPrediction.NaiveBayes import NaiveBayes
import sys
import nltk as n

def train_model (model_filename, data_filename):
	bayes_model = NaiveBayes()
	result = KBTU.load_examples(data_filename)
	if result == False:
		return False
	X = []
	for res in result[0]:
		example = []
		for x in res:
			example.extend([x[1]])
		X.extend([example])
	Y = result[1]
	bayes_model.train(X, Y)
	return bayes_model.store(model_filename)


def load_model(model_filename):
	bayes_model = NaiveBayes()
	bayes_model.load(model_filename)
	return bayes_model
	
def predict(model, sentence):
	pos_tokens = n.pos_tag(n.word_tokenize(sentence))
	sentence_tokens = KBTU.split_input_into_examples(pos_tokens)
	keywords = []
	X = []
	for x in sentence_tokens:
		example = []
		for y in x:
			example.extend([y[1]])
		X.extend([example])
	for x in range(len(X)):
		if model.predict(X[x]) == '1':
			#if not sentence_tokens[x][1][0] in keywords:
			keywords.extend([sentence_tokens[x][1][0]])
	return keywords
	
#model = load_model("file")

#data = openFile("trainDat3.txt")
#sentences = []
#for training_case in data:
	#sentences.extend([training_case[3].split(":")[1].lstrip().rstrip()])
	#sentences.extend([training_case[4].split(":")[1].lstrip().rstrip()])

#file = open("keyword-sentenceOutput.txt", 'w')
	
#for sentence in sentences:
	#file.write(str(sentence + "\n"))
	#file.write(str(str(predict(model, sentence.lower())) + "\n\n"))
#file.close()
	
#sentence = input("Please enter your sentence to have keyworded:\n--->")
#while "exit" not in sentence.lower():
	#print(predict(model, sentence.lower()))
	#sentence = input("Please enter your sentence to have keyworded:\n--->")