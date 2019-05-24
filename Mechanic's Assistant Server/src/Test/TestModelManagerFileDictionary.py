class TestModelManagerFileDictionary:

    def TestAddProblemPredictorFilePath(self):
        mmfd = ModelManagerFileDictionary()
        assert len(mmfd.fileDict) == 0
        filePath = "../problemPredictorFile.py"
        mmfd.addProblemPredictorFilePath(filePath)
        assert len(mmfd.fileDict) == 1
        assert mmfd.getProblemPredictorFilePath() == filePath