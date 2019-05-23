import src.KeywordPrediction.KeywordBayesTrainingUtils as KBTU
from src.KeywordPrediction.NaiveBayes import NaiveBayes
import sys
import nltk as n

class NaiveBayesKeywordPredictor:
    
    def __init__(self):
        self.naiveBayesModel = NaiveBayes()
    
    def trainModel(self, dataFilename : str):
        result = KBTU.loadExamples(data_filename)
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

    def storeModel(self, outFileName : str):
        self.naiveBayesModel.store(outFileName)

    def loadModel(self, modelFileName : str):
        self.naiveBayesModel.load(modelFileName)

    def predict(self, sentence):
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
            if self.naiveBayesModel.predict(X[x]) == '1':
                #if not sentence_tokens[x][1][0] in keywords:
                keywords.extend([sentence_tokens[x][1][0]])
        return keywords
