class ModelManagerFileDictionary(object):
    """Stores the paths for the files containing the models responsible for the following:
    KeywordGrouping
    KeywordPrediction
    ProblemPrediction
    """

    def __init__(self):
        self.fileDict = {}
        self.expectedEntries = [
            "ProblemPredictor",
            "KeywordPredictor",
            "KeywordClusterer",
            "KeywordData",
            "MainData"
        ]

    def addProblemPredictorFilePath(self, filePath : str) -> None:
        self.fileDict["ProblemPredictor"] = filePath

    def addKeywordPredictorFilePath(self, filePath : str) -> None:
        self.fileDict["KeywordPredictor"] = filePath

    def addKeywordClustererFilePath(self, filePath : str) -> None:
        self.fileDict["KeywordClusterer"] = filePath

    def addFormattedKeywordDataFilePath(self, filePath : str) -> None:
        self.fileDict["KeywordData"] = filePath

    def addFormattedExampleQueriesFilePath(self, filePath : str) -> None:
        self.fileDict["MainData"] = filePath

    def getProblemPredictorFilePath(self) -> str:
        return self.fileDict["ProblemPredictor"]

    def isReady(self):
        for (expectedEntry in expectedEntries):
            if not expectedEntry in self.fileDict:
                return False
        return True