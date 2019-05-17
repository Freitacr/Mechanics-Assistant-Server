from src.Util import JimDataLoader
from sys import argv
import json
from io import StringIO

def addToList(dataElement : str, dictionary : dict, key : str):
    if (dataElement == "None"):
        dictionary[key] = None
        return
    dictionary[key] = dataElement

if __name__ == "__main__":
    data = JimDataLoader.openFile(argv[1])
    jsonData = []
    for dataElement in data:
        jsonDict = {}
        addToList(dataElement[0], jsonDict, "make")
        addToList(dataElement[1], jsonDict, "model")
        addToList(dataElement[2], jsonDict, "VIN")
        addToList(dataElement[3], jsonDict, "complaint")
        addToList(dataElement[4], jsonDict, "problem")
        jsonData.append(jsonDict)
    outFile = open(argv[1], "w")
    json.dump(jsonData, outFile)
    outFile.flush();
    outFile.close();
    