from typing import List
from typing import abstractmethod

class KeywordClusterer:
    
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
    def predict(keywords : List[str]) -> list:
        pass