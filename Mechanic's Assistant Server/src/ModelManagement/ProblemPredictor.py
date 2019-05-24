from typing import List, abstractmethod

class ProblemPredictor:
    """Provides uniform way to access models for predicting problems based on queries"""

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

