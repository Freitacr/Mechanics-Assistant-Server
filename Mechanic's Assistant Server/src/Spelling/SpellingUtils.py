from autocorrect import spell
import math
class CustomSpellingDictionary:

	def __init__(self):
		self.spelling_dictionary = {}

	def __character_similarity(self, strA, strB):
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
		difference = math.sqrt((len(strB) - len(strA)) ** 2)
		modifier = 1 - (.25 * difference)
		if modifier < 0:
			return 0
		else: 
			return modifier
		
	def store(self, filename):
		file = open(filename, "w")
		for entry in self.spelling_dictionary.items():
			file.write(entry[0] + "=" + str(entry[1]) + "\n")
		file.close()
		
	def load(self, filename):
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
		
	def __map_characters(self, str, dict):
		for x in range(len(str)):
			if str[x] in dict:
				dict[str[x]] = dict[str[x]] + 1
			else:
				dict[str[x]] = 1
				
	def __sequence_similarity(self, strA, strB):
		correct_characters = 0
		iter_amount = len(strB)
		if len(strA) < iter_amount:
			iter_amount = len(strA)
		for character_index in range(iter_amount):
			if strB[character_index] == strA[character_index]:
				correct_characters += 1
		return correct_characters / len(strB)
		
	def add(self, str):
		str = str.lower()
		if str in self.spelling_dictionary:
			self.spelling_dictionary[str] = self.spelling_dictionary[str] + 1
		else:
			self.spelling_dictionary[str] = 1
			
	def correct(self, str):
		if len(str) <= 1:
			return spell(str)
		str = str.lower()
		candidates = self.__getFromList(str)
		if len(candidates) == 0:
			return spell(str)
		else:
			highest_index = -1
			highest_score = 0
			total_occurences = 0
			for entry in self.spelling_dictionary.items():
				if (entry[0] in candidates):
					total_occurences += entry[1]
			keys = []
			for key in self.spelling_dictionary.keys():
				if key in candidates:
					keys.extend([key])
			for index in range(len(keys)):
				letter_similarity = self.__character_similarity(str, keys[index])
				seq_similarity = self.__sequence_similarity(str, keys[index])
				if (seq_similarity == 0):
					continue
				len_mod = self.__length_difference_modifier(str, keys[index])
				if (len_mod == 0):
					continue
				
				occurence_score = self.spelling_dictionary[keys[index]] / total_occurences
				current_score = ((letter_similarity + seq_similarity) / 2) * occurence_score
				if seq_similarity == 1.0:
					current_score *= 2
				current_score *= len_mod
				if current_score > highest_score:
					highest_score = current_score
					highest_index = index
			return keys[highest_index]
			
	def __getFromList(self, str):
		print("\n")
		candidates = []
		for candidate in self.spelling_dictionary.keys():
			threshold = 1
			if len(candidate) < 2:
				continue
			elif len(candidate) < 3:
				threshold = 1/2
			elif len(candidate) < 4:
				threshold = 2/3
			else:
				threshold = 3/4
			if self.__character_similarity(str, candidate) >= threshold:
				if not self.__sequence_similarity(str, candidate) == 0:
					candidates.extend([candidate])
					print (str, candidate, self.__character_similarity(str, candidate), self.__sequence_similarity(str, candidate), threshold)
		print (candidates, "\n")
		return candidates
	
	def __str__(self):
		return str(self.spelling_dictionary)