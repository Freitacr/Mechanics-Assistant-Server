import nltk as nl
from src.Util.JimDataLoader import openFile

def split_input_into_examples (pos_tokens_list):
    ret = []
    tokens_extended = [("START", "START")]
    tokens_extended.extend(pos_tokens_list)
    tokens_extended.extend([("END", "END")])
    num_of_examples = len(tokens_extended) - 2
    for x in range(num_of_examples):
        ret.extend([tokens_extended[x : x+3]])
    return ret

def loadExamples(fileName : str):
    X = []
    Y = []
    examples = openFile(fileName)
    for exampleDict in examples:
        containedData = exampleDict["data"]
        example = []
        for dataDict in containedData:
            word = dataDict["word"]
            partOfSpeech = dataDict["pos"]
            example.append((word, partOfSpeech))
        isCorrect = "1" if exampleDict["correct"] else "0"
        X.append(example)
        Y.append(isCorrect)
    return X, Y