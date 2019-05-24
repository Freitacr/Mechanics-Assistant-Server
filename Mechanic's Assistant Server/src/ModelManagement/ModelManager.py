from enum import Enum
from typing import List
from os import path
from src.ModelManagement.KeywordClusterer import KeywordClusterer
from src.ModelManagement.KeywordPredictor import KeywordPredictor
from src.ModelManagement.ProblemPredictor import ProblemPredictor
from src.ModelManagement.ModelFileDictionary import ModelManagerFileDictionary

class ModelManager(object):
    """Class responsible for providing easy access to and training and usage of the three types of predictors used in predicting querys"""
    
    __fileExtentionMappingDict = {
        PredictorEnum.KEYWORD_PREDICTOR : {},
        PredictorEnum.KEYWORD_CLUSTERER : {},
        PredictorEnum.PROBLEM_PREDICTOR : {}
    }
    
    class PredictorEnum(Enum):
        KEYWORD_PREDICTOR=0
        KEYWORD_CLUSTERER=1
        PROBLEM_PREDICTOR=2

    @classmethod
    def register(cls, fileExtention, correspondingClass, classType : PredictorEnum) -> None:
        '''Registers the Class specified by correspondingClass and the fileExtension associated with it with this class'''
        cls.__fileExtentionMappingDict[classType][fileExtention] = correspondingClass

    def __init__(self, modelManagerFileDict : ModelManagerFileDictionary):
        self.__modelManagerFileDict = modelManagerFileDict
        self.__keywordPredictor : KeywordPredictor = None
        self.__keywordClusterer : KeywordClusterer = None
        self.__problemPredictor : ProblemPredictor = None
        
    def __loadModels(self) -> None:
        
        pass

    def predict(self, query, n : int) -> List[str]:
        pass

    def trainModels(self) -> bool:
        pass

    def areModelsTrained(self) -> bool:
        '''Returns True if all modelsFiles listed in modelFileDict exist, False otherwise'''
        for key, modelFilePath in self.__modelManagerFileDict.items():
            if not (path.exists(modelFilePath)):
                return False
        return True
    



