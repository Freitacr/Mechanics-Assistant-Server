from autocorrect import spell
import math
class CustomSpellingDictionary:

    def __init__(self):
        self.spelling_dictionary = {}

    def __character_similarity(self, strA, strB):
        '''Calculates and returns a percentage similarity for strA and strB based on the number of each character they contain'''
        dict_a = {}
        dict_b = {}
        self.__map_characters(strA, dict_a)
        self.__map_characters(strB, dict_b)
        correct_characters = 0
        for key in dict_b.keys():
            if key in dict_a:
                if dict_a[key] > dict_b[key]:
                    correct_characters += dict_b[key]
                elif dict_a[key] < dict_b[key]:
                    correct_characters += dict_a[key]
                else:
                    correct_characters += dict_b[key]
            else:
                continue
        return correct_characters / len(strB)
    
    def __length_difference_modifier(self, strA, strB):
        '''Calculates and returns a value to modify the similarity between strA and strB.
        this is based on the difference between their lengths'''
        #Calculate the absolute value of the difference between the length of a and b
        difference = abs(len(strB) - len(strA))
        #if the differnce is greater than four letters
        #we do not allow it to be selected as the correct spelling of the word
        #otherwise, we return the calculated modifier
        modifier = 1 - (.25 * difference)
        if modifier < 0:
            return 0
        else: 
            return modifier
        
    def store(self, filename):
        '''Stores this custom spelling dictionary to the file specified by filename'''
        file = open(filename, "w")
        for entry in self.spelling_dictionary.items():
            file.write(entry[0] + "=" + str(entry[1]) + "\n")
        file.close()
        
    def load(self, filename):
        '''Loads this custom spelling dictionary from the file specified by filename'''
        self.__init__()
        try:
            file = open(filename, "r")
        except FileNotFoundError:
            return False
        for line in file:
            split = line.split("=")
            self.spelling_dictionary[split[0]] = int(split[1])
        file.close()
        return True
        
    def __map_characters(self, strIn, dict):
        '''Adds all characters in str as keys to the dictionary specified by dict
        The values associated with the keys are the number of times the character occurs in the string'''
        for x in range(len(strIn)):
            if strIn[x] in dict:
                dict[strIn[x]] = dict[strIn[x]] + 1
            else:
                dict[strIn[x]] = 1
                
    def __sequence_similarity(self, strA, strB):
        '''Calculates the similarity between string a and b based on the sequence their characters occur in'''
        correct_characters = 0
        iter_amount = len(strB)
        if len(strA) < iter_amount:
            iter_amount = len(strA)
        for character_index in range(iter_amount):
            if strB[character_index] == strA[character_index]:
                correct_characters += 1
        return correct_characters / len(strB)
        
    def add(self, strIn):
        '''Adds the string specified by str to this dictionary
        if it already exists, then increment the count of how many instances have been seen'''
        strIn = strIn.lower()
        if strIn in self.spelling_dictionary:
            self.spelling_dictionary[strIn] = self.spelling_dictionary[strIn] + 1
        else:
            self.spelling_dictionary[strIn] = 1

    def __calcOccurencesCandidates(self, candidates :list) -> int:
        '''Calculates the sum of the occurences of each of the candidate strings in candidates'''
        occurences = 0
        for candidate in candidates:
            if candidate in self.spelling_dictionary:
                occurences += self.spelling_dictionary[candidate]
        return occurences

    def __calcCandidateScore(self, candidate : str, actual : str, total_occurences : int) -> float:
        '''Calculates a score for the candidate string given by candidate
        This score is based off the character and sequence similarity,
        and modified by the difference of length between them'''
        letter_similarity = self.__character_similarity(actual, candidate)
        seq_similarity = self.__sequence_similarity(actual, candidate)
        if (seq_similarity == 0):
            return 0
        len_mod = self.__length_difference_modifier(actual, candidate)
        if (len_mod == 0):
            return 0
                
        occurence_score = self.spelling_dictionary[candidate] / total_occurences
        current_score = ((letter_similarity + seq_similarity) / 2) * occurence_score
        if seq_similarity == 1.0:
            current_score *= 2
        current_score *= len_mod
        return current_score
            
    def correct(self, strIn) -> str:
        '''Attempts to autocorrect the string given by strIn'''
        if len(strIn) <= 1:
            return spell(strIn)
        strIn = strIn.lower()
        candidates = self.__getFromList(strIn)
        if len(candidates) == 0:
            #We have no idea what this word could be, just autocorrect it
            return spell(strIn)
        else:
            highest_index = -1
            highest_score = 0
            total_occurences = self.__calcOccurencesCandidates(candidates)
            keys = candidates.copy() #This is valid as the list of candidates is generated from the dictionary to begin with
            #Calculate the score for each candidate, and keep track of the largest one
            for index in range(len(keys)):
                current_score = self.__calcCandidateScore(keys[index], strIn, total_occurences)
                if current_score > highest_score:
                    highest_score = current_score
                    highest_index = index
            return keys[highest_index]
            
    def __getFromList(self, strIn : str):
        '''Generates a list of words that are candidates for being the correct spelling of strIn'''
        candidates = []
        for candidate in self.spelling_dictionary.keys():
            #Establish a similarity threshold that each candidate must meet to be considered
            threshold = 1
            if len(candidate) < 2:
                continue
            elif len(candidate) < 3:
                threshold = 1/2
            elif len(candidate) < 4:
                threshold = 2/3
            else:
                threshold = 3/4
            if self.__character_similarity(strIn, candidate) >= threshold:
                #Candidate passed a basic character similarity, now we must make sure it has SOME relation 
                #to the original spelling of the word
                if not self.__sequence_similarity(strIn, candidate) == 0:
                    candidates.extend([candidate])
                    print (strIn, candidate, self.__character_similarity(strIn, candidate), self.__sequence_similarity(strIn, candidate), threshold)
        return candidates
    
    def __str__(self):
        '''ToString method'''
        return str(self.spelling_dictionary)