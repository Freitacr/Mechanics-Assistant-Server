import operator
from math import log
import operator
#
class Group:
    #main method
    def __init__(self, keyword):
        self.contained_members = []
        self.contained_keywords = {}
        self.prev_keywords = {}
        self.selected_keywords = [keyword]
        
    #Adds a member to the given group
    def addMembers(self, data):
        self.contained_members = []
        for example in data:
            if not example in self.contained_members:
                num_same_keywords = self.__num_keywords(example)
                if num_same_keywords == len(self.selected_keywords):
                    self.contained_members.extend([example])
        self.prev_keywords = self.contained_keywords
    
    #updates group "name" or the keywords that the group is known by
    def update_selected_keywords(self, max_group_size):
        self.__update_contained_keywords()
        global_threshold = 1 - ((len(self.contained_members) / max_group_size) * (1 - (len(self.selected_keywords) / 4)))
        
        for keyword_entry in self.contained_keywords.items():
            if keyword_entry[1] / len(self.contained_members) >= global_threshold and keyword_entry[0] not in self.selected_keywords:
                self.selected_keywords.extend([keyword_entry[0]])
                break #take out of loop in order to not add two keywords at once
        self.addMembers(self.contained_members)

    #number of keywords in the group
    def __num_keywords(self, example):
        ret = 0
        for keyword in self.selected_keywords:
            if keyword in example:
                ret += 1
        return ret
    
    #boolan if the group has changed
    def changed_after_update(self):
        if (not self.prev_keywords == {} and not self.contained_keywords == {}):
            return not self.prev_keywords == self.contained_keywords
        else:
            return True
    
    #grabs all keywords that the members have
    def __update_contained_keywords(self):
        self.contained_keywords = {}
        for member in self.contained_members:
            for keyword in member:
                if keyword in self.contained_keywords:
                    self.contained_keywords[keyword] = self.contained_keywords[keyword] + 1
                else:
                    self.contained_keywords[keyword] = 1
    
    #how similar the given keyword is to the group
    def getSimilarityScore(self, example):
        if len(example) == 0:
            return 0
        sim_keywords = self.__num_keywords(example)
        return sim_keywords / len(example)

    #set selected_keywords to the given list
    def setSelectedKeywords(self, keywords):
        self.selected_keywords = keywords
    
    
    
class Cluster:    
        #main function
        def __init__(self):
            self.groups = []
        #trains the cluster model
        def train (self, data):
            min_members = 1.2 ** (log(len(data)) / (log(1.88)))
            min_members -= 3
            
            top_keywords = self.__getKKeywordsFromData(data)
            groups = self.__getGroupsFromKeywords(data, top_keywords, min_members)
            self.groups = groups
            #self.order_groups()
            
        
        #put the groups in order based on similarity
        def order_groups(self):
            ordered_groups = []
            ranking_lists = []
            index_dictionary = {}
            index = 0
            #setup ranking lists. These will be converted momentarily to ranking dictionaries
            for group in self.groups:
                index_dictionary[group] = index 
                ranking_list = []
                comp_group_index = 0
                for comp_group in self.groups:
                    ranking_list.extend([[comp_group, 0, comp_group_index]])
                    #if groups are the same, assign the pairing an impossible values
                    if comp_group == group:
                        ranking_list[-1][1] = -1
                        continue
                    #otherwise count the members and update the middle value in what was extended to ranking_list last
                    for member in group.contained_members:
                        if member in comp_group.contained_members:
                            ranking_list[-1][1] = ranking_list[-1][1] + 1
                    comp_group_index += 1 #keep track of indices
                #sort from largest to smallest
                ranking_list = sorted(ranking_list, key=operator.itemgetter(1), reverse=True)
                ranking_lists.extend([ranking_list])
                index += 1 #move on to the next group
            
            #Translate the ranking lists into dictionaries. The values are dependent on the length of
            #self.groups - 1 (which is the same as ranking_lists[ranking_list_index] below
            for ranking_list_index in range(len(ranking_lists)):
                temp_dict = {}
                for member_index in range(len(ranking_lists[ranking_list_index])):
                    #because I'm lazy (and this is likely the fastest way to handle this oversight now)
                    #and since the last element in each ranking list is guarenteed at this point
                    #to be the group's own self, setting its score to -1 will make it impossible
                    #for any pairing with this group and itself
                    if member_index == len(ranking_lists[ranking_list_index]) - 1:
                        sim_score = -1
                    else:
                        sim_score = (len(ranking_lists[ranking_list_index]) - 1) - member_index
                        sim_score /= (len(ranking_lists[ranking_list_index]) - 1)
                    temp_dict[ranking_lists[ranking_list_index][member_index][0]] = sim_score
                ranking_lists[ranking_list_index] = temp_dict
            
            
            #now to do the actual ordering. The idea behind this is that each group should have a say
            #proportional to their distance from the new member's index (aka the length of the list
            #so far) in who the new member should be, which will be based on their ranking dictionary
            
            ordered_groups.extend([self.groups[0]])
            while not len(ordered_groups) == len(self.groups):
                match_scores = []
                for potential in self.groups:
                    score = 0.0
                    element_index = 0 #normally I would make the loop have this index, but the indexing
                                      #is already too long and complex at this point.
                    for element in ordered_groups:
                        #if this is already a doomed pairing (as it will have already been in the list)
                        #then just skip the calculations
                        #surprisingly, this is more efficient time-wise than maintaining a pool of 
                        #potential matches
                        if score == -1:
                            continue
                        #breaking this down: The index dictionary stores the index of a group's
                        #ranking dictionary using the group itself as a key.
                        #That dictionary contains the preference scores for each group, using the
                        #group as a key
                        sim_score = ranking_lists[index_dictionary[element]][potential]
                        #if the sim_score is -1, then the group is already present in the ordered list
                        #and this pairing should be made to be impossible
                        if sim_score == -1:
                            score = -1
                            continue
                        #otherwise calculate the amount of "say" this group has in the decision and
                        #update the similarity score accordingly
                        sim_score *= (element_index + 1) / len(ordered_groups)
                        score += sim_score
                    match_scores.extend([[potential, score]])
                #extend the last element in the sorted match_scores list's first element. This is the
                #actual group. This ensures that ordered_groups will only contain Group objects
                ordered_groups.extend([ sorted(match_scores, key=operator.itemgetter(1))[-1][0] ])
            
            #reassign self.groups with the now ordered list, and the algorithm is done
            self.groups = ordered_groups
        
        #Returns examples that are not in a group
        def __getDefaultExamples(self, group_list, data):
            default_examples = []
            for example in data:
                default = True
                for group in group_list:
                    if not group.getSimilarityScore(example) == 0:
                        default = False
                if default:
                    default_examples.extend([example])
            return default_examples
        
        #returns K keywords
        def __getKKeywordsFromData(self, data):
            ret = []
            for keyword in self.__getDataKeywords(data).items():
                self.__addKeywordToList(ret, keyword)
                
            for index in range(len(ret)):
                ret[index] = ret[index][0] #don't need the amount of the keyword seen
            return ret
            
        #removes groups that are the same selected_keywords
        def __removeSameGroups (self, groups):
            for group in groups:
                if group == None:
                    continue
                for groupB_index in range(len(groups)):
                    if groups[groupB_index] == None or group == groups[groupB_index]:
                        continue
                    if self.__areSameGroup(group, groups[groupB_index]):
                        groups[groupB_index] = None
            tempGroups = []
            for group in groups:
                if not group == None:
                    tempGroups.extend([group])
            return tempGroups
            
            
            
        #creates a list of group based on the keywords from data
        def __getGroupsFromKeywords(self, data, keywords, min_members):
            groups = []
            defaults = self.__getDefaultExamples(groups, data)
            self.__getGroups(data, keywords, defaults, groups, min_members)
            prev_groups = []
            changed = True
            loop_count = 0
            while (changed):
                groups = self.__removeSameGroups(groups)
                identical = True
                
                for group in groups:
                    contained = False
                    for prev_group in prev_groups:
                        if self.__areSameGroup(group, prev_group):
                            contained = True
                    if not contained:
                        identical = False
                        break
                if identical and loop_count > min_members:
                    break
                    
                changed = False
                max_group_size = self.__getMaxGroupSize(groups)
                for group_index in range(len(groups)):
                    temp1 = []
                    temp1.extend(groups[group_index].selected_keywords)
                    groups[group_index].update_selected_keywords(max_group_size)
                    if not temp1 == groups[group_index].selected_keywords:
                        changed = True
                        max_group_size = self.__getMaxGroupSize(groups)
                        self.__getGroups(data, keywords, defaults, groups, min_members)
                
                prev_groups = []
                prev_groups.extend(groups)
                loop_count += 1
            
            actual_groups = []
            for group in groups:
                if not group == None and not len(group.contained_members) < min_members:
                    actual_groups.extend([group])
            return actual_groups
            
        #size of group with the most members
        def __getMaxGroupSize(self, groups):
            max_group = 0
            for group in groups:
                if (len(group.contained_members) > max_group):
                    max_group = len(group.contained_members)
            return max_group
        
        #returns if the groups are the same
        def __areSameGroup(self, groupA, groupB):
            if not (len(groupA.selected_keywords) == len(groupB.selected_keywords)):
                return False
            for keyword in groupA.selected_keywords:
                if not keyword in groupB.selected_keywords:
                    return False
            return True
            
        #is the group in the list
        def __inGroupList(self, group, group_list):
            for groupB in group_list:
                if self.__areSameGroup(group, groupB):
                    return True
            return False
            
        #creates groups from data
        def __getGroups(self, data, keywords, defaults, group_list, min_members):
            avg_group_size = 0.0

            while (len(defaults) > avg_group_size):
                cont_search = True
                keyword_index = 0
                while (True):
                    tempGroup = Group(keywords[keyword_index])
                    tempGroup.addMembers(data)
                    if len(tempGroup.contained_members) > min_members and not self.__inGroupList(tempGroup, group_list):
                        group_list.extend([tempGroup])
                        break
                    else:
                        keyword_index += 1
                        if (keyword_index == len(keywords)):
                            cont_search = False
                            break
                defaults = self.__getDefaultExamples(group_list, data)
                keywords = self.__getKKeywordsFromData(defaults)
                avg_group_size = self.__getAvgGroupSize(group_list)
                if not cont_search:
                    break
            
        #gets all of the keywords within the data
        def __getDataKeywords(self, data):
            ret = {}
            for example in data:
                for keyword in example:
                    if keyword in ret:
                        ret[keyword] = ret[keyword] + 1
                    else:
                        ret[keyword] = 1
            return ret
        
        #sets avg_group_size
        def __getAvgGroupSize(self, group_list):
            temp = 0
            for group in group_list:
                    temp += len(group.contained_members)
            avg_group_size = temp / len(group_list)
            return avg_group_size
        
        #insertion sort for keyword
        def __addKeywordToList(self, keywords_array, keyword_entry):
            if len(keywords_array) == 0:
                keywords_array.extend([keyword_entry])
            inserted = False
            for keyword_entry_index in range(len(keywords_array)):
                if keyword_entry[1] > keywords_array[keyword_entry_index][1]:
                    holding_var = self.__insert_into_array(keywords_array, keyword_entry, keyword_entry_index)
                    keywords_array.extend([holding_var])
                    inserted = True
                    break
            if not inserted:
                keywords_array.extend([keyword_entry])
                        
        #puts keyword into the array using the index found in __addKeywordToList
        def __insert_into_array(self, keywords_array, keyword_entry, at_index):
            holding_var = keyword_entry
            temp = None
            for index in range(at_index, len(keywords_array), 1):
                temp = keywords_array[index]
                keywords_array[index] = holding_var
                holding_var = temp
            return holding_var
            
        #writes a storage file
        def store(self, filename):
            file = None
            try:
                file = open(filename, "w")
            except FileNotFoundError:
                return False
            for group in self.groups:
                toPrint = ""
                for keyword in group.selected_keywords:
                    toPrint +=str(keyword) + " "
                file.write(toPrint.rstrip() + "\n")
            file.close()
        
        #loads the storage file
        def load(self, filename):
            file = None
            try:
                file = open(filename, 'r')
            except FileNotFoundError:
                return False
            self.__init__()
            for line in file:
                lineSplit = line.lstrip().rstrip().split(" ")
                group = Group("")
                group.setSelectedKeywords(lineSplit)
                self.groups.extend([group])
                
        #returns group most similar to keyword
        def predict(self, keywords):
            sim_scores = []
            for group_index in range(len(self.groups)):
                sim_score = self.groups[group_index].getSimilarityScore(keywords)
                if not sim_score == 0:
                    sim_scores.extend([(group_index + 1, sim_score)])
            if len(sim_scores) == 0:
                sim_scores.extend([(0,0)])
            sim_scores = (list(reversed(sorted(sim_scores, key=operator.itemgetter(1)))))
            for score_index in range(len(sim_scores)):
                sim_scores[score_index] = sim_scores[score_index][0]
            return sim_scores
        #return the n groups most similar to keyword
        def predict_top_n(self, keywords, n):
            sim_scores = self.predict(keywords)
            if len(sim_scores) < n:
                for x in range(n - len(sim_scores)):
                    sim_scores.extend([0])
            return sim_scores[:n]