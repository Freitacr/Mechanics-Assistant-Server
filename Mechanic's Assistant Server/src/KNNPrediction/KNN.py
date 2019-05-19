import operator
import math
from typing import List
class KNN:
    '''Implementation of the K Nearest Neighbours Machine Learning Algorithm'''
    
    def __init__(self):
        self.label_mapping_dicts = []
        self.data_points = []
    
    def train(self, X, Y):
        '''Trains the model, which effectively just means storing all of the points into the model'''
        self.setup_label_mapping(X)
        for example_index in range(len(X)):
            self.data_points.extend([[self.transform_example(X[example_index]), Y[example_index]]])

    def store(self, filePath):
        '''Stores the model into the file specified by filePath'''
        file = None
        try:
            file = open(filePath, "w")
        except FileNotFoundError:
            return False
        for point in self.data_points:
            xstr = ""
            for x in point[0]:
                xstr += str(x) + " "
            file.write(xstr.rstrip())
            file.write("\t" + str(point[1][0]) + "\n")
        file.write("dictBegin\n")
        for dict in self.label_mapping_dicts:
            if dict == None:
                file.write("None\n")
            else:
                dictString = ""
                for entry in dict.items():
                    dictString += str(entry[0]) + "=" + str(entry[1]) + "|"
                file.write(dictString[:-1] + "\n")
        file.close()
        return True

    def load(self, filePath):
        '''Loads the model from the file specified by filePath'''
        self.__init__()
        file = None
        try:
            file = open(filePath, "r")
        except FileNotFoundError:
            return False
        dictSection = False
        dict_index = 0
        for line in file:
            if line == "dictBegin\n":
                dictSection = True
                continue
            if not dictSection:
                lineSplit = line.split("\t")
                xlist = lineSplit[0].split(" ")
                for xindex in range(len(xlist)):
                    xlist[xindex] = int(xlist[xindex])
                self.data_points.extend([[xlist, [lineSplit[1].rstrip()]]])
            else:
                if line == "None\n":
                    self.label_mapping_dicts.extend([None])
                    dict_index += 1
                    continue
                lineSplit = line.split("|")
                self.label_mapping_dicts.extend([{}])
                for ex in lineSplit:
                    exSplit = ex.split("=")
                    self.label_mapping_dicts[dict_index][exSplit[0]] = int(exSplit[1])
                dict_index += 1
        file.close()
        return True

    def predict(self, x, k, distance_measure=0, function = None):
        '''Predicts the n most similar points to x, based on the distance between x and the each point
        @param x: the data to make a prediction for
        @param k: the number of most similar points to return
        @param distance_measure: value to determine which distance measure is used
            0: use euclidian_distance
            1: use manhattan_distance
            2: use the function specified by function
        @param function: function used to calculate the distance between two points
            Only used if distance_measure is 2'''
        if distance_measure == 2 and function == None:
            raise ValueError("Must supply a custom function for comparing points")
        transformed_x = self.transform_example_volitile(x)
        point_storage = []
        for point in self.data_points:
            distance = 0.0
            if distance_measure == 0:
                distance = self.euclidian_distance(transformed_x, point[0])
            elif distance_measure == 2:
                distance = function(transformed_x, point[0])
            else:
                distance = self.manhattan_distance(transformed_x, point[0])
            point_storage.extend([[point, distance]])
        point_storage = sorted(point_storage, key=operator.itemgetter(1))
        return point_storage[:k]
            
    def euclidian_distance(self, x : List[float], y : List[float]) -> float:
        '''Calculates the euclidian distance between the points x and y'''
        if not len(x) == len(y):
            raise ValueError("Lengths of inputs must be identical for distance calculation")
        ret = 0.0
        for axis_index in range(len(x)):
            ret += (y[axis_index] - x[axis_index]) ** 2
        return ret ** (1/2)
        
    def manhattan_distance(self, x : List[float], y : List[float]) -> float:
        '''Calculates the distance between the points x and y using manhattan distance'''
        if not len(x) == len(y):
            raise ValueError("Lengths of inputs must be identical for distance calculation")
        ret = 0.0
        for axis_index in range(len(x)):
            ret += (math.fabs(y[axis_index] - x[axis_index]))
        return ret
    
    def setup_label_mapping(self, X):
        self.label_mapping_dicts = [None] * len(X[0])
        for index in range(len(X[0])):
            for example in X:
                need_mapping = False
                try:
                    float(example[index])
                except ValueError:
                    need_mapping = True
                if need_mapping:
                    self.label_mapping_dicts[index] = {}
                    break
                
        for dict_index in range(len(self.label_mapping_dicts)):
            if not self.label_mapping_dicts[dict_index] == None:
                for example in X:
                    if not example[dict_index].lower() in self.label_mapping_dicts[dict_index]:
                        self.label_mapping_dicts[dict_index][example[dict_index].lower()] = len(self.label_mapping_dicts[dict_index])
    
    def transform_example(self, x):
        new_example = []
        for dict_index in range(len(self.label_mapping_dicts)):
            if not self.label_mapping_dicts[dict_index] == None:
                new_example.extend([self.label_mapping_dicts[dict_index][x[dict_index].lower()]])
            else:
                new_example.extend([x[dict_index]])
        return (new_example)

    def transform_example_volitile(self, x):
        transformed_x = []
        for dict_index in range(len(self.label_mapping_dicts)):
            if not self.label_mapping_dicts[dict_index] == None:
                try:
                    transformed_x.extend([self.label_mapping_dicts[dict_index][x[dict_index].lower()]])
                except KeyError:
                    transformed_x.extend([len(self.label_mapping_dicts[dict_index])])
            else:
                transformed_x.extend([x[dict_index]])
        return transformed_x