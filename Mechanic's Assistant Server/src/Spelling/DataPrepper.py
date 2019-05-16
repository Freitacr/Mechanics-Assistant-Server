from src.Spelling import CustomSpellingDictionary
from src.Util.JimDataLoader import openFile
from autocorrect import spell

data = openFile("trainDat3.txt")

SpellingDictionary = CustomSpellingDictionary()
SpellingDictionary.load("cusDictionary.txt")

for dataIndex in range(len(data)):
	tempData = []
	make = data[dataIndex][0]
	model = data[dataIndex][1]
	vin = data[dataIndex][2]
	cusComplaintSplit = data[dataIndex][3].split(":")
	causeSplit = data[dataIndex][4].split(":")
	cusComplaint = cusComplaintSplit[1]
	cause = causeSplit[1]
	causeSplit = cause.lstrip().rstrip().lower().split(" ")
	cusComplaintSplit = cusComplaint.lstrip().rstrip().lower().split(" ")
	for index in range(len(causeSplit)):
		temp = causeSplit[index]
		if ',' in temp:
			causeSplit[index] = temp.replace(',', '')
			temp = causeSplit[index]
		if '.' in temp:
			causeSplit[index] = temp.replace('.', '')
			temp = causeSplit[index]
		if not (spell(temp) == temp):
			causeSplit[index] = SpellingDictionary.correct(causeSplit[index])
		if not len(temp) <= 1:
			SpellingDictionary.add(temp)
	for index in range(len(cusComplaintSplit)):
		temp = cusComplaintSplit[index]
		if ',' in temp:
			cusComplaintSplit[index] = temp.replace(',', '')
			temp = cusComplaintSplit[index]
		if '.' in temp:
			cusComplaintSplit[index] = temp.replace('.', '')
			temp = cusComplaintSplit[index]
		if not (spell(temp) == temp):
			cusComplaintSplit[index] = SpellingDictionary.correct(cusComplaintSplit[index])
		if not len(temp) <= 1:
			SpellingDictionary.add(temp)
	print(causeSplit)
	print(cusComplaintSplit)

SpellingDictionary.store("cusDictionary.txt")
