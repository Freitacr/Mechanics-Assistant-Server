def openFile(filename):
	file = open(filename, "r")
	data = []
	for line in file:
		lineData = []
		
		for x in line.split("\t"):
			if not x == '':
				lineData.extend([x])
		data.extend([lineData])
	for index in range(len(data)):
		data[index] = [s.lstrip() for s in data[index]]
	file.close()
	return data
	