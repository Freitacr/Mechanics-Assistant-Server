import nltk as nl

def split_input_into_examples (pos_tokens_list):
    ret = []
    tokens_extended = [("START", "START")]
    tokens_extended.extend(pos_tokens_list)
    tokens_extended.extend([("END", "END")])
    num_of_examples = len(tokens_extended) - 2
    for x in range(num_of_examples):
        ret.extend([tokens_extended[x : x+3]])
    return ret

def setup_training_examples_for_sentence(examples, original_sentence):
    keywords = input("Please input the keywords for the sentence \"{0}\" with spaces between each key word\n-->".format(original_sentence))
    keywords_split = nl.word_tokenize(keywords.lstrip().rstrip().lower())
    sentence_words = nl.word_tokenize(original_sentence.lower())
    
    complete = False
    while not complete:
        complete = True
        for word in keywords_split:
            if word not in sentence_words:
                complete = False
                keywords = input("keyword \"{0}\" was not in the original sentence, please re enter all keywords\n-->".format(word))
                keywords_split = nl.word_tokenize(keywords.lstrip().rstrip().lower())
                continue
    
    
    ret = []
    for word_index in range(len(sentence_words)):
        if sentence_words[word_index] in keywords_split:
            ret.extend(["1"])
        else:
            ret.extend(["0"])
    return examples, ret
    
def store_examples(filename, X, Y):
    file = None
    try:
        file = open(filename, "w")
    except FileNotFoundError:
        return False
    for x in range(len(X)):
        examples_string = ""
        for y in range(len(X[x])):
            examples_string += str(X[x][y][0]) + "|" + str(X[x][y][1] + " ")
        file.write(examples_string + str(Y[x]) + "\n")
    file.close()
    
    
def load_examples(filename):
    file = None
    try:
        file = open(filename, "r")
    except FileNotFoundError:
        return False
    X = []
    Y = []
    for line in file:
        lineSplit = line.split(" ")
        Y.extend(lineSplit[-1].lstrip().rstrip())
        example = []
        for tupleString in lineSplit[:-1]:
            tupleStringSplit = tupleString.split("|")
            if tupleStringSplit[0] == '' and tupleStringSplit[1] == '':
                tupleStringSplit = [',',',']
            example.extend([(tupleStringSplit[0], tupleStringSplit[1])])
        X.extend([example])
    return X, Y