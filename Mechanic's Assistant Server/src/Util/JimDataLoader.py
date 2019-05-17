import json

def openFile(filename):
    file = open(filename, "r")
    data = json.load(file)
    return data