import json

def openFile(filename):
    '''Loads the contents of the file specified by filename and attempts to translate them to json'''
    file = open(filename, "r")
    data = json.load(file)
    file.close()
    return data