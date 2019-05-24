from typing import List
from typing import abstractmethod

class KeywordPredictor:
    '''Abstract class to standardize the access to models that deal with predicting keywords of sentences'''
    def __init__():
        pass
    
    @abstractmethod
    def train(X, Y):
        pass

    @abstractmethod
    def store(modelFilePath : str) -> bool:
        pass

    @abstractmethod
    def load(modelFilePath : str) -> bool:
        pass

    @abstractmethod
    def predict(sentence : str) -> List[str]:
        pass