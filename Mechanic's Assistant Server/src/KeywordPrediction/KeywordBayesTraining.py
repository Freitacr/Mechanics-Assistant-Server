from src.Util.JimDataLoader import openFile
from src.KeywordPrediction import KeywordBayes, KeywordBayesTrainingUtils as KBTU
import sys
import os
import nltk as n

if __name__ == "__main__":
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
        sentences.extend([training_case["complaint"].split(":")[1].lstrip().rstrip()])
        sentences.extend([training_case["problem"].split(":")[1].lstrip().rstrip()])
    X, Y = [[], []]
    for sentence in sentences:
        tokens = n.word_tokenize(sentence)
        tags = n.pos_tag(tokens)
        examplesList = KBTU.split_input_into_examples(tags)
        examples, value = KBTU.setup_training_examples_for_sentence(examplesList, sentence)
        X.extend(examples)
        Y.extend(value)
    KBTU.store_examples("temp.dat", X, Y)
    KeywordBayes.train_model("src/KeywordPrediction/" + modelfilename, "temp.dat")
    os.remove("temp.dat")