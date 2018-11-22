import codecs
from datetime import datetime
import csv
import sys
import pandas as pd
import xlsxwriter
#from pandas import ExcelWriter
#from openpyxl import Workbook

from pathlib import Path

# Switches
create_pts = False
metric = False

# Set Variables - SI or Metric
Machine_compaction_zone = 0.5  # for an 836
cellToArea = 1.244  # A cell is 34 cm cubed
AreatoAcre = cellToArea / 43560
cellVoltoCubic = cellToArea / 27
double_handle_threshold = -2.0  # used to define rework based on vertical change down
thick_lift = 2.5  # used to measure how often too much material is placed
filter_low_volume_cell = 0.0  # used to refine area of operation by removing cells with little volume
filter_elevation_uncertainty = 0.0  # used to keep passes with little elevation change out of compaction and lift statistics

if metric == True:
    Machine_compaction_zone = 0.5  # for an 836
    cellToArea = 1.244
    AreatoAcre = cellToArea / 43560
    cellVoltoCubic = cellToArea / 27
    double_handle_threshold = 3.0
    thick_lift = 2.5
    filter_low_volume_cell = 0.5



if sys.argv[1] == "help":
    print("""
    USAGE: python xxxxx.py input_file_name
    This tool will write output to a file called input_file_name.CCA""")
    sys.exit(0)

filename = sys.argv[1]
current_file =  Path(filename).stem

#############################################################
# read CSV file into dictionaly grouped by unique NE values #
#############################################################
northEastDict = {}
cell_pass_count_total = 0
with codecs.open(filename, mode="r", encoding="utf-8-sig") as csvFile:
    
    for csvRow in csv.DictReader(csvFile):
        cell_pass_count_total = cell_pass_count_total + 1  # total number of cell passes in the file
        northEastData = {
            "dateTime": datetime.strptime(csvRow["Time"], "%Y/%b/%d %H:%M:%S.%f"),
            "machine": csvRow["Machine"],
            "elevation": csvRow["Elevation_FT"],
            "passNumber": csvRow["PassNumber"],
            "delta_from_last_pass": 0.0,
            "EventMagnitude:": 0.0,
        }
        northEast = (float(csvRow["CellN_FT"]), float(csvRow["CellE_FT"]))
        northEastDict.setdefault(northEast, []).append(northEastData)
    # End of for csvRow in csvReader:
# End of with open(filename) as csvFile:
# created a dictionary grouped by NE grid

#######################################
# Start of determining Machines and operating time #
#######################################

contributing_machines = set()
total_duration = 0.0
machine_duration = 0.0

# find contributing machines
for k, dk in northEastDict.items(): # k is the current cell being looked at ?
    for x in dk: # x is the ?
        # find which machines worked in the time period
        contributing_machines.add(x['machine'])

#################################################check this logic.....
machine_operation_dict = {}
for machine in contributing_machines:
    start_time = None
    stop_time = None
    start_hour = None
#Find start and stop time for each machine
    for k, dk in northEastDict.items(): # k is the current cell being looked at ?
        for x in dk: # x is the ? # for all keys
            if machine == x['machine']:
                DateTimeSTR = str(x['dateTime'])
                sppos = DateTimeSTR.find(' ')
                current_time = DateTimeSTR[10+1:]
                current_date = DateTimeSTR[:10]
                if start_time is None or start_time > current_time:
                    start_time = current_time
                    start_hour = start_time[:2]
                    start_minute = start_time[3:5]

                if stop_time is None or stop_time < current_time:
                    stop_time = current_time
                    stop_hour = stop_time[:2]
                    stop_minute = stop_time[3:5]
    
                if int(stop_minute) < int(start_minute):
                    stop_hour = int(stop_hour) - 1
                    stop_minute = int(stop_minute) + 60
                duration_hour = int(stop_hour) - int(start_hour)
                duration_minute = (int(stop_minute) - int(start_minute))/60
                machine_duration = duration_hour + duration_minute
    machine_operation_data = {
        "duration": machine_duration,
        "start": start_time,
        "stop": stop_time,
        }
    machine_operation_dict.setdefault(machine,[]).append(machine_operation_data)

    total_duration = float(total_duration) + float(machine_duration)


############################
# CELL SUMMARY INFORMATION #
############################
bin_pass_counts = dict() # This is to calc 1 pass cells and to compare passcounts to VL

############################
#Create max min surface files
############################
# Top surface Output file
if create_pts == True:
    TopSurfaceOut = "./output/" + current_file + "_Top.pts"
    TopOut = open(TopSurfaceOut,'w')
    T = csv.writer(TopOut, delimiter=',',dialect='excel',lineterminator='\n')

# Bottom surface Output file
    BottomSurfaceOut = "./output/" + current_file + "_Bottom.pts"
    BottomOut = open(BottomSurfaceOut,'w')
    B = csv.writer(BottomOut, delimiter=',',dialect='excel',lineterminator='\n')

################################
# Find the volume of each cell and add summary dictionary to cells with more than one pass #
################################


for k, dk in northEastDict.items():
    max_elevation = None
    min_elevation = None
    first_elevation = None
    last_elevation = None
    max_pass_count = None
    cell_volume = 0.0
    cell_cut_volume = 0.0
    cell_fill_volume = 0.0
    
    for x in dk: # find first and last pass elevations to calculate cell volume (cut or fill)
        if first_elevation is None or int(x['passNumber']) == 1:
            first_elevation = float(x['elevation'])
           
        if max_pass_count is None or  int(max_pass_count) <=  int(x['passNumber']):
            max_pass_count = x['passNumber']
            last_elevation = float(x['elevation'])
   
    cell_volume = float(last_elevation) - float(first_elevation)
 

    NEcoordinate = str(k)
    comma = NEcoordinate.find(',')
    East = NEcoordinate[(comma + 2):-1]
    North = NEcoordinate[1:(comma - 1)]
   
   # put data into pointcloud files
    if create_pts == True:
       T.writerow([East, North, last_elevation])
       B.writerow([East,North,first_elevation ])
   

# seperate into cut/fill and total
    if cell_volume < (filter_low_volume_cell * -1):
        cell_cut_volume = cell_volume
    
    if cell_volume > filter_low_volume_cell:
        cell_fill_volume = cell_volume

    # add summary dictionary and insert cut/fill, volume and maximum passcount informaion 
    if(int(max_pass_count) > 1): # only add summary information to cells with more than one pass
        summaryData = {
            "passes_for_cell": int(max_pass_count),
            "volume": cell_volume,
            "cut": cell_cut_volume,
            "fill": cell_fill_volume,
            "volume_event": 0.0,
            "cnt_over_compaction": 0,
            "cnt_thick_lift": 0,
            "cnt_under_compaction": 0,
            "air_space_utilization":0.0,
            "efficiency_index":0.0,
           
        }
        
# append summary information to current NE key in northEastDict dictionary
        northEastDict[k].append(summaryData)
        
## bin passcount occurances into a dictionary (to compare with VL and to tally single pass cells)
    
    bin_pass_counts[max_pass_count] = bin_pass_counts.get(max_pass_count,0) + 1

#END - Find the volume of each cell # 
if create_pts == True:   
    TopOut.close()
    print("wrote data to {}".format(TopSurfaceOut))
    BottomOut.close()
    print("wrote data to {}".format(BottomSurfaceOut))

###################
# summarize and evaluate individual cells #
###################
cnt_Unique_cell_compaction_area = 0
bin_0 = bin_1 = bin_2 = bin_3 = bin_4 = bin_5 = bin_6 = 0
neg_1 = neg_2 = neg_3 = neg_4 = neg_5 = neg_6 = 0
active_0 = active_1 = active_2 = active_3 = active_4 = active_5 = active_6 = 0
active_volume_1 = active_volume_2 = active_volume_3 = active_volume_4 = active_volume_5 = active_volume_6 = 0
active_volume_list =[]
positive_passes = 0
negative_passes = 0
lift_passes = 0
compaction_passes = 0

positive_passes_greater_than_filter = 0
active_cnt = 0
cnt_low_volume_cells = 0
cnt_cut_cells = 0
cnt_cut_passes = 0
cnt_low_volume_passes = 0
cnt_lift_uncertainty = 0
cnt_compact_uncertainty = 0
double_handle_cubic = 0.0
remediation_under_compaction_events = 0.0



for k, dk in northEastDict.items():
    #PassElevDict = {} 
    PassElevList = list() 
    for vol in dk:  # determine if cell compaction should be evaluated for specific cell based on volume of cell
        current_elevation = None
        event_elevation_bottom_active_material = {}
        if "passes_for_cell" in vol: # cells only have summary info if they have more than one pass - pull out summary information
            volume = vol['volume']
            passes_for_cell = vol['passes_for_cell']

            if (float(volume) < 0):
                cnt_cut_cells = cnt_cut_cells + 1
                cnt_cut_passes = cnt_cut_passes + passes_for_cell
            
            if (float(volume) <= filter_low_volume_cell) and (float(volume) >= 0):
                cnt_low_volume_cells = cnt_low_volume_cells + 1
                cnt_low_volume_passes = cnt_low_volume_passes + passes_for_cell
# this is in the wrong place?
                
            if (float(volume) > filter_low_volume_cell): # filter out cells with little volume from surface area but not volume calcs
                cnt_Unique_cell_compaction_area = cnt_Unique_cell_compaction_area + 1 # these are the remaining unique cells which were not filtered out
                compaction_state = 1  
                active_material = 0.0 
                event_elevation_counter = 0
                event_elevation_bottom = []
                reverse_elevent_elevation_bottom = []
# create a pass and elevation dictionary for current cell  !!!!Is this ordered correctly?              
                for elev in dk:  # pull out pass and elevation data and put in a pass and elevation dictionary  
                    if "passNumber" in elev: # pull out pass and elevation information for evaluation         
                        if current_elevation == None:
                            previous_elevation = elev['elevation']
                        current_elevation = elev['elevation']
                        delta_elevation = float(current_elevation) - float(previous_elevation)
                        previous_elevation = current_elevation # this is used to get the bottom of an elevation event
                        elev['delta_from_last_pass'] = delta_elevation
                        layer = delta_elevation
                        PassElevList.append(elev['elevation'])
    

####################################
# Single cell Evaluate compaction  #
####################################

                        #for layer in elev_delta_list:
# This area evalutes layers and records under compaction and Thick lift events for a single cell
# - Full compaction state = 1.  Under compaction states are 0  
# - Under compaction event is defined as placing material on top of under compacted material.  
# - Compactiuon state goes to 1  when pass to pass compaction is between zero and the (-) MCZ value

##################
# Movement Down  #
##################   
# Compaction state is set to complete with movement down less than MCZ value            
                        if layer <= 0.0 :  #evaluate compaction layers
                            if layer >= -(Machine_compaction_zone): # this is the on only condition that sets compaction state to 1
                                if (compaction_state == 1): # Fully compacted material was compacted more
                                    if layer < -filter_elevation_uncertainty:
                                        vol['cnt_over_compaction'] = vol['cnt_over_compaction'] + 1 # keep track of overcompaction effort
                                compaction_state = 1 # set as compacted
                                active_material = 0  # compaction is complete, no active material
                                #print("reset compaction zone down")
                                
                            else: # moved down more than MCZ, this is normal compaction
                                compaction_state = 0  # every time compaction is more than MCZ, full compaction is required
                                active_material = active_material + layer # Active material reduces because layer is negative

                                # CHECK if movement down mitigated an under compaction event
                                if (active_material < 0.0):  # movement down is larger than the lift, material may have been removed
                                    # Compare the current elevation to the list of "bottom" of events.  If the current elevation is lower than a bottom, 
                                    # find the active material associated with that event and add it to the mitigation tally.  
                                    # Also pop the elevation of the event of the list and remove it from teh dictionaty
                                    if len(event_elevation_bottom) > 0:
                                        reverse_elevent_elevation_bottom = list(reversed(event_elevation_bottom))
                                        
                                    for bottom in (event_elevation_bottom):
                                        if float(current_elevation) < float(bottom) :  ## I don't think this works ???
                                            #print(current_elevation, " current elevation < bottom", bottom)
                                            #if bottom in event_elevation_bottom:
                                                #thing = event_elevation_bottom[str(bottom)]
                                                #print("found it", thing)
                                            
                                            remediation = event_elevation_bottom_active_material[bottom]
                                            elev['EventMagnitude'] = -remediation #Assign magnitude of event to pass of machine
                                            #if remediation > 1.0:
                                               # print(remediation)
                                            #print(event_elevation_bottom)
                                            popped = event_elevation_bottom.pop()
                                            #print(" current elev, popped", current_elevation,popped)
                                            remediation_under_compaction_events = remediation_under_compaction_events + remediation
                                       # remove remediated event from event magnitude distribution
                                       # figure out which machine removed the event???     
                                            active_cnt = active_cnt - 1    
                                            if remediation <= Machine_compaction_zone: 
                                                active_1 = active_1 - 1
                                                active_volume_1 = active_volume_1 - remediation
                                            elif remediation > (Machine_compaction_zone) and remediation <= (Machine_compaction_zone * 2):
                                                active_2 = active_2 - 1
                                                active_volume_2 = active_volume_2 - remediation
                                            elif remediation > (Machine_compaction_zone * 2) and remediation <= (Machine_compaction_zone * 3):
                                                active_3 = active_3 - 1
                                                active_volume_3 = active_volume_3 - remediation
                                            elif remediation > (Machine_compaction_zone * 3) and remediation <= (Machine_compaction_zone * 4):
                                                active_4 = active_4 - 1
                                                active_volume_4 = active_volume_4 - remediation
                                            elif remediation > (Machine_compaction_zone * 4) and remediation <= (Machine_compaction_zone * 5):
                                                active_5 = active_5 - 1
                                                active_volume_5 = active_volume_5 - remediation
                                            elif remediation >= (Machine_compaction_zone * 5):
                                                active_6 = active_6 - 1
                                                active_volume_6 = active_volume_6 - remediation                                 

                                    active_material = 0.0  
                                    #print("____________________________")  
                                    #print("reset active material < 0")       
                                                    
################
# Movement UP  # 
################     
# This is where event volumes occur, if material is placed on top of uncompacted material, the active material is the event volume                       
                        if layer > 0.0:
                            if layer <= Machine_compaction_zone:    # Material added within MCZ
                                if compaction_state == 1: # compaction was complete, no under compaction event
                                    active_material = 0  
                            elif layer > (Machine_compaction_zone) and layer < (thick_lift): #Material added more than MCZ but less than TL
                                if compaction_state == 1:  # compaction was complete, no under compaction event
                                    active_material = layer
                                    #print(active_material)
                                    compaction_state = 0  # movement was up greater than MCZ so reset compaction state 
                                else: # UNDER COMPACTION EVENT >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                                    vol['cnt_under_compaction'] = vol['cnt_under_compaction'] + 1 # put under compaction event information into summary dictionary          
                                    vol['volume_event'] = vol['volume_event'] + active_material # add up total volume of all evants for cell
                                    elev['EventMagnitude'] = active_material #Assign magnitude of event to pass of machine
                                    event_adjusted_for_active = float(previous_elevation) - active_material # this gets us to the bottom of the previous lift
                                    event_elevation_bottom.append(event_adjusted_for_active) 
                                    event_elevation_counter = event_elevation_counter + 1
                                    # add event bottom and active material to a dictionary so that if mitigated, the active material contribution to under
                                    # compacted volume can be added to the undercompacted event mitigation tally
                                    event_elevation_bottom_active_material[event_adjusted_for_active] = active_material
                                    #print (event_elevation_bottom_active_material)

                                 #  END of UNDER COMPATION EVENT  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
    
                                    active_cnt = active_cnt + 1    #???
                                    if active_material <= Machine_compaction_zone: 
                                        active_1 = active_1 + 1
                                        active_volume_1 = active_volume_1 + active_material
                                    elif active_material > (Machine_compaction_zone) and active_material <= (Machine_compaction_zone * 2):
                                        active_2 = active_2 + 1
                                        active_volume_2 = active_volume_2 + active_material
                                    elif active_material > (Machine_compaction_zone * 2) and active_material <= (Machine_compaction_zone * 3):
                                        active_3 = active_3 + 1
                                        active_volume_3 = active_volume_3 + active_material
                                    elif active_material > (Machine_compaction_zone * 3) and active_material <= (Machine_compaction_zone * 4):
                                        active_4 = active_4 + 1
                                        active_volume_4 = active_volume_4 + active_material
                                    elif active_material > (Machine_compaction_zone * 4) and active_material <= (Machine_compaction_zone * 5):
                                        active_5 = active_5 + 1
                                        active_volume_5 = active_volume_5 + active_material
                                    elif active_material >= (Machine_compaction_zone * 5):
                                        active_6 = active_6 + 1
                                        active_volume_6 = active_volume_6 + active_material
                                   
                                    active_material =  layer # + active_material ???  Only layer becasue active material has been recorded as under compacted event
                                   
                        
                            else: # A thick lift occured 
                                if compaction_state == 1: # compaction was complete, no under compaction event
                                    active_material = layer
                                    compaction_state = 0 # movement was up greater than MCZ so reset compaction state
                                else: # UNDER COMPACTION EVENT >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                                    vol['cnt_under_compaction'] = vol['cnt_under_compaction'] + 1 # put under compaction event information into summary dictionary
                                    vol['volume_event'] = vol['volume_event'] + active_material # put under compaction event information into summary dictionary
                                    elev['EventMagnitude'] = active_material #Assign magnitude of event to pass of machine
                                    event_adjusted_for_active = float(previous_elevation) - active_material # this gets us to the bottom of the previous lift
                                    event_elevation_bottom.append(event_adjusted_for_active) 
                                    event_elevation_counter = event_elevation_counter + 1
                                    # add event bottom and active material to a dictionary so that if mitigated, the active material contribution to under
                                    # compacted volume can be added to the undercompacted event mitigation tally
                                    event_elevation_bottom_active_material[event_adjusted_for_active] = active_material
                                    #print ("thick",event_elevation_bottom_active_material)
                                #  END of UNDER COMPATION EVENT  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                                    #print("before",active_material)
                                    active_cnt = active_cnt + 1    
                                    if active_material <= Machine_compaction_zone: 
                                        active_1 = active_1 + 1
                                        active_volume_1 = active_volume_1 + active_material
                                    elif active_material > (Machine_compaction_zone) and active_material <= (Machine_compaction_zone * 2):
                                        active_2 = active_2 + 1
                                        active_volume_2 = active_volume_2 + active_material
                                    elif active_material > (Machine_compaction_zone * 2) and active_material <= (Machine_compaction_zone * 3):
                                        active_3 = active_3 + 1
                                        active_volume_3 = active_volume_3 + active_material
                                    elif active_material > (Machine_compaction_zone * 3) and active_material <= (Machine_compaction_zone * 4):
                                        active_4 = active_4 + 1
                                        active_volume_4 = active_volume_4 + active_material
                                    elif active_material > (Machine_compaction_zone * 4) and active_material <= (Machine_compaction_zone * 5):
                                        active_5 = active_5 + 1
                                        active_volume_5 = active_volume_5 + active_material
                                    elif active_material >= (Machine_compaction_zone * 5):
                                        active_6 = active_6 + 1
                                        active_volume_6 = active_volume_6 + active_material      
                                                                
                                    active_material = layer # + active_material ???  Only layer becasue active material has been recorded as under compacted event
                                    #print(active_material)
                      
                                vol['cnt_thick_lift'] = vol['cnt_thick_lift'] + 1 # put thick lift information into summary dictionary
                    # end of up movement evaluation
################
# #Layer summary
################    
                        if layer <= 0.0 :  #evaluate compaction layers
                            negative_passes = negative_passes + 1
                            if layer > -(Machine_compaction_zone) and layer < -filter_elevation_uncertainty:
                                neg_1 = neg_1 + 1
                            elif layer < -(Machine_compaction_zone) and layer >= -(Machine_compaction_zone * 2):
                                neg_2 = neg_2 + 1
                            elif layer < -(Machine_compaction_zone * 2) and layer >= -(Machine_compaction_zone * 3):
                                neg_3 = neg_3 + 1
                            elif layer < -(Machine_compaction_zone * 3) and layer >= -(Machine_compaction_zone * 4):
                                neg_4 = neg_4 + 1
                            elif layer < -(Machine_compaction_zone * 4) and layer >= -(Machine_compaction_zone * 5):
                                neg_5 = neg_5 + 1
                            elif layer <= -(Machine_compaction_zone * 5):
                                neg_6 = neg_6 + 1
                            # one more else to catch the rest....
                            if (layer > -(filter_elevation_uncertainty)):
                                cnt_compact_uncertainty = cnt_compact_uncertainty + 1
                            if(layer < double_handle_threshold):
                                    double_handle_cubic = double_handle_cubic + layer

                        if layer > 0.0:
                            positive_passes = positive_passes + 1
                            if layer <= filter_elevation_uncertainty: 
                                cnt_lift_uncertainty = cnt_lift_uncertainty + 1
                                bin_0 = bin_0 + 1
                            elif filter_elevation_uncertainty < layer <= Machine_compaction_zone: 
                                bin_1 = bin_1 + 1
                            elif layer > (Machine_compaction_zone) and layer <= (Machine_compaction_zone * 2):
                                bin_2 = bin_2 + 1
                            elif layer > (Machine_compaction_zone * 2) and layer <= (Machine_compaction_zone * 3):
                                bin_3 = bin_3 + 1
                            elif layer > (Machine_compaction_zone * 3) and layer <= (Machine_compaction_zone * 4):
                                bin_4 = bin_4 + 1
                            elif layer > (Machine_compaction_zone * 4) and layer <= (Machine_compaction_zone * 5):
                                bin_5 = bin_5 + 1
                            elif layer >= (Machine_compaction_zone * 5):
                                bin_6 = bin_6 + 1
                  
############################
# End Evaluate compaction  #
############################

############################
##Determine what time events took place and what machines were involved for Excel outputs
############################

blah_index = 0
# events per hour by machine
machine_volume_eventDict = {"mitigated_volume" :  {} , }
machine_volume_eventDict_positive = {"unmitigated_volume" : {},}
machine_volume_eventDict_negative = {"remediation_volume" : {}, }

machine_area_eventDict = {"mitigated_area" :  {} ,}
machine_area_eventDict_positive = {"unmitigated_area" :  {} ,}
machine_area_eventDict_negative = {"remediation_area" :  {} ,}

machine_lift_Dict = {"lifts" : {},}

for current_machine in contributing_machines:
    blah = list(contributing_machines)
    check_machine = blah[blah_index]
    blah_index = blah_index + 1
    machine_volume_events_all = {}
    machine_volume_events_positive = {}
    machine_volume_events_negative = {}
    machine_area_events_all = {}
    machine_area_events_negative = {}
    machine_area_events_positive = {}
    lift_summary = {'0 to 0.5':0,'B':0,'C':0,'D':0,'E':0,'F':0}
    eventMagnitude = 0.0

    # seperate events by machine, for each machine tally unmitigated volume, mitigated volume and remediation volume
    for k, dk in northEastDict.items():
        for data_point in dk: 
            if "machine" in data_point:
                if check_machine == (data_point['machine']):    
                    if "EventMagnitude" in data_point: 
                        current_time = (data_point['dateTime'])

                        # this takes (+) and (-) to determine the mitigated volume
                        if current_time.hour not in machine_volume_events_all:
                            machine_area_events_all[current_time.hour] = 1
                            machine_volume_events_all[current_time.hour] = data_point['EventMagnitude']
                        else:
                            machine_volume_events_all[current_time.hour] = machine_volume_events_all[current_time.hour] + data_point['EventMagnitude']
                        
                        # this records all positive volume events for a total of all volume events
                        if data_point['EventMagnitude'] > 0.0:
                            if current_time.hour not in machine_volume_events_positive:
                                machine_volume_events_positive[current_time.hour] = data_point['EventMagnitude']
                                machine_area_events_positive[current_time.hour]  = 1
                                machine_area_events_all[current_time.hour] = 1
                            else:
                                machine_volume_events_positive[current_time.hour] = machine_volume_events_positive[current_time.hour] + data_point['EventMagnitude']
                                machine_area_events_positive[current_time.hour]  += 1
                                machine_area_events_all[current_time.hour] += 1

                        # this records all remediation volume events
                        if data_point['EventMagnitude'] <  0.0:
                            if current_time.hour not in machine_volume_events_negative:
                                machine_volume_events_negative[current_time.hour] =  data_point['EventMagnitude']
                                machine_area_events_negative[current_time.hour]  = 1
                                machine_area_events_all[current_time.hour] = 1
                            else:
                                machine_volume_events_negative[current_time.hour] = machine_volume_events_negative[current_time.hour] + data_point['EventMagnitude']
                                machine_area_events_negative[current_time.hour]  +=1
                                machine_area_events_all[current_time.hour] -= 1
                   
                    # get data for lift graph
                    lift_magnitude = data_point['delta_from_last_pass']
                    if lift_magnitude > 0.0:
                            
                            if filter_elevation_uncertainty < lift_magnitude <= Machine_compaction_zone: 
                                lift_summary['0 to 0.5'] += 1
                                
                            elif lift_magnitude > (Machine_compaction_zone) and lift_magnitude <= (Machine_compaction_zone * 2):
                                lift_summary['B'] += 1
                             
                            elif lift_magnitude > (Machine_compaction_zone * 2) and lift_magnitude <= (Machine_compaction_zone * 3):
                                lift_summary['C'] += 1
                                
                            elif lift_magnitude > (Machine_compaction_zone * 3) and lift_magnitude <= (Machine_compaction_zone * 4):
                                lift_summary['D'] += 1
                                
                            elif lift_magnitude > (Machine_compaction_zone * 4) and lift_magnitude <= (Machine_compaction_zone * 5):
                                lift_summary['E'] += 1
                                
                            elif lift_magnitude >= (Machine_compaction_zone * 5):
                                lift_summary['F'] += 1
                         
    # this is machine breakout data
# lift
    machine_lift_Dict.setdefault(current_machine,[]).append(lift_summary)
    #print(machine_lift_Dict)
# volume
    machine_volume_eventDict.setdefault(current_machine,[]).append(machine_volume_events_all)       # mitigated
    machine_volume_eventDict_positive.setdefault(current_machine,[]).append(machine_volume_events_positive) # unmitigated
    machine_volume_eventDict_negative.setdefault(current_machine,[]).append(machine_volume_events_negative) # remediation
# area  
    machine_area_eventDict.setdefault(current_machine,[]).append(machine_area_events_all) # mitigated
    machine_area_eventDict_positive.setdefault(current_machine,[]).append(machine_area_events_positive) # unmitigated
    machine_area_eventDict_negative.setdefault(current_machine,[]).append(machine_area_events_negative) # remediation
    
    
# total events per hour
# total event volume per
hour_volume_events_all = {}
hour_volume_events_positive = {}
hour_volume_events_negative = {}
eventMagnitude = 0.0

hour_area_events_all = {}
hour_area_events_positive = {}
hour_area_events_negative = {}


# Tally all events (mitigated volume)
# tally events unmitigated TBD
# tally events remediation TBD

for k, dk in northEastDict.items():
    for data_point in dk:  
        if "EventMagnitude" in data_point: 
            current_time = (data_point['dateTime'])

            #All
            if current_time.hour not in hour_volume_events_all: 
                #hour_area_events_all[current_time.hour] = 1
                hour_volume_events_all[current_time.hour] = data_point['EventMagnitude']
            else:
                hour_volume_events_all[current_time.hour] = hour_volume_events_all[current_time.hour] + data_point['EventMagnitude']

            #Negative
            if current_time.hour not in hour_volume_events_negative: 
                #hour_area_events_negative[current_time.hour] = 1
                hour_volume_events_negative[current_time.hour] = data_point['EventMagnitude']
            else:
                if data_point['EventMagnitude'] <= 0.0:
                    hour_volume_events_negative[current_time.hour] = hour_volume_events_negative[current_time.hour] + data_point['EventMagnitude']
                       
            #Positive
            if current_time.hour not in hour_volume_events_positive: 
                #hour_area_events_positive[current_time.hour] = 1
                hour_volume_events_positive[current_time.hour] = data_point['EventMagnitude']
            else:
                if data_point['EventMagnitude'] >= 0.0:
                    hour_volume_events_positive[current_time.hour] = hour_volume_events_positive[current_time.hour] + data_point['EventMagnitude']
        
            

machine_volume_eventDict['mitigated_volume'] = [hour_volume_events_all]
machine_volume_eventDict_positive['unmitigated_volume'] = [hour_volume_events_positive] 
machine_volume_eventDict_negative['remediation_volume'] = [hour_volume_events_negative]

machine_area_eventDict['mitigated_area'] = [hour_area_events_all]
machine_area_eventDict_positive['unmitigated_area'] = [hour_area_events_positive]
machine_area_eventDict_negative['remediation_area'] = [hour_area_events_negative]


############################
## End Determine what time events took place and what machines were involved
############################

#####################################################################################################################################
# Begin summary  # pull information and get sums for whole dataset from individual cell summary dictionarys
#####################################################################################################################################    
# passcount occurances - put into a list to sort
lst = list()
for k,v in(sorted(bin_pass_counts.items())):
    #print (k , "Passes " ,v, " occurances ")
    lst.append((int(k),v))
lst.sort()

for k,v in lst[:1]:
    single_pass_cells = v
# End passcount sorting



#################################################
# Create Over,Under and thicklift surface files #
##################################################
# Over compaction surface file
if create_pts == True:
    OverSurfaceOut =  "./output/" + current_file + "_OverCompaction.pts"
    OverOut = open(OverSurfaceOut,'w')
    O = csv.writer(OverOut, delimiter=',',dialect='excel',lineterminator='\n')

# Under compction surface file
    UnderSurfaceOut = "./output/" + current_file + "_Under_compaction.pts"
    UnderOut = open(UnderSurfaceOut,'w')
    U = csv.writer(UnderOut, delimiter=',',dialect='excel',lineterminator='\n')

# Thicklift surface file
    ThickSurfaceOut = "./output/" + current_file + "_ThickLift.pts"
    ThickOut = open(ThickSurfaceOut,'w')
    Th = csv.writer(ThickOut, delimiter=',',dialect='excel',lineterminator='\n')

# Volume Event surface file
    VolumeEventSurfaceOut = "./output/" + current_file + "_VolumeEvent.pts"
    VolumeEventOut = open(VolumeEventSurfaceOut,'w')
    VE = csv.writer(VolumeEventOut, delimiter=',',dialect='excel',lineterminator='\n')

# Coverage surface file
    CoverageSurfaceOut = "./output/" + current_file + "_coverage.pts"
    CoverageOut = open(CoverageSurfaceOut,'w')
    CO = csv.writer(CoverageOut, delimiter=',',dialect='excel',lineterminator='\n')

# Active surface file
    ActiveSurfaceOut = "./output/" + current_file + "_ActiveArea.pts"
    ActiveOut = open(ActiveSurfaceOut,'w')
    ACT = csv.writer(ActiveOut, delimiter=',',dialect='excel',lineterminator='\n')


# tally totals from indiviual cells
total_cnt_thick = 0
total_cnt_under = 0
total_cnt_over = 0
total_volume = 0.0
total_cut = 0.0
total_fill = 0.0
total_event_volume = 0.0

for k, dk in northEastDict.items():
    NEcoordinate = str(k)
    comma = NEcoordinate.find(',')
    East = NEcoordinate[(comma + 2):-1]
    North = NEcoordinate[1:(comma - 1)]
   
    for summary in dk:     
        if "passes_for_cell" in summary: # determine if cell compaction is present to be evaluated 
            if create_pts == True:
                CO.writerow([East,North,0.0 ])
            # Pull volume information for each cell 
            current_cell_volume = float(summary['volume'])
            if current_cell_volume >= filter_low_volume_cell and create_pts == True:
                ACT.writerow([East,North,current_cell_volume ])

            current_cell_cut = float(summary['cut'])
            current_cell_fill = float(summary['fill']) 
            current_event_volume = float(summary['volume_event'])

            # Populate VolumeEvent surface
            if current_event_volume > 0 and create_pts == True:
                VE.writerow([East,North,current_event_volume ])

            # total volume information
            total_volume = float(total_volume) + float(current_cell_volume)
            total_cut = float(total_cut) + float(current_cell_cut)
            total_fill = float(total_fill) + float(current_cell_fill)
            total_event_volume = float(total_event_volume) + float(current_event_volume)

            # pull cell stats
            current_cnt_over = summary['cnt_over_compaction']
            current_cnt_under = summary['cnt_under_compaction']
            current_cnt_thick = summary['cnt_thick_lift']

            # Populate over, under and thick surface files
            if current_cnt_over > 0 and create_pts == True:
                O.writerow([East,North,current_cnt_over ])
            if current_cnt_under > 0 and create_pts == True:
                U.writerow([East,North,current_cnt_under ])
            if current_cnt_thick > 0 and create_pts == True:
                Th.writerow([East,North,current_cnt_thick ])
            

            # total cell stats
            total_cnt_thick = total_cnt_thick + current_cnt_thick
            total_cnt_under = total_cnt_under + current_cnt_under
            total_cnt_over = total_cnt_over + current_cnt_over

if create_pts == True:
    ActiveOut.close()
    print("wrote data to {}".format(ActiveSurfaceOut))
    CoverageOut.close()
    print("wrote data to {}".format(CoverageSurfaceOut))
    OverOut.close()
    print("wrote data to {}".format(OverSurfaceOut))
    UnderOut.close()
    print("wrote data to {}".format(UnderSurfaceOut))
    ThickOut.close()
    print("wrote data to {}".format(ThickSurfaceOut))
    VolumeEventOut.close()
    print("wrote data to {}".format(VolumeEventSurfaceOut))




# end tally totals from indivudual cells


#####################################################################################################################################
# Begin writing output
#####################################################################################################################################    
print("**********************************************************************************************************************************************")
print("file processed: ", filename)
print("Variables set :", thick_lift, "Thick lift threshold ")
print("Variables set :", filter_low_volume_cell, "Minimum volume filter")
print("Variables set :", Machine_compaction_zone, "Compaction ratio ")
print("Cell to sq ft conversion 1.244")
print("**********************************************************************************************************************************************")
print("Machines contributing :",len(contributing_machines))
#for v in contributing_machines:
#    print(v)
#print("Start Time : ",start_time)
#print("Stop  Time : ",stop_time)
#print("%.3f" % machine_duration, "hours")
print("%.2f" % total_duration, "hours")

for k, dk in machine_operation_dict.items():
    for x in dk:
        print(k,"start time :",x['start'],"stop time :", x['stop'], "duration :","%.2f" % x['duration'])


print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Area - Site summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>") 
print("Machine Operation Coverage area   :", "%.4f" % (cell_pass_count_total * AreatoAcre), " acres")
area_coverage = (len(northEastDict) * AreatoAcre)
print("Coverage - Area(VL uses)          :", "%.4f" % (area_coverage), " acres")
single_pass_area = single_pass_cells * AreatoAcre
print("Removed for single pass           :", "%.4f" %  (single_pass_area), " acres" )
area_cut_cell = cnt_cut_cells * AreatoAcre
print("Removed for Cut                   :" ,"%.4f" % (area_cut_cell), " acres") 
print("Variables set :", filter_low_volume_cell, "Minimum volume filter")
area_low_volume_cell = cnt_low_volume_cells * AreatoAcre
print("Removed for low volume            :","%.4f" % (area_low_volume_cell), " acres")
active_compaction_area = (cnt_Unique_cell_compaction_area * AreatoAcre)
print("Active compaction - area          :", "%.4f" % (active_compaction_area), " acres")
#print("Area check                        :", (area_coverage - single_pass_area - area_low_volume_cell - area_cut_cell))

print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Passes - Site summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>") 
print("Total pass                        :", cell_pass_count_total)
print("Removed for single pass           :", single_pass_cells  )
print("Removed for low volume            :", cnt_low_volume_passes)
print("Removed for Cut                   :", cnt_cut_passes)
print("Positive                          :", positive_passes)
print("Negative                          :", negative_passes)
#print("check : ",(cell_pass_count_total - single_pass_cells - cnt_low_volume_passes - cnt_cut_passes  - positive_passes - negative_passes))
print("Variables set :", filter_elevation_uncertainty, " elevation_uncertainty")
print("filter lift                       :" , cnt_lift_uncertainty)
print("filter compaction                 :" , cnt_compact_uncertainty)
lift_passes = positive_passes - cnt_lift_uncertainty
print("lift                              :", lift_passes)
compaction_passes = negative_passes - cnt_compact_uncertainty
print("Compaction                        :", compaction_passes)
print("Pass check                        :")



print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Airspace Performance summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>") 

F = float(total_fill * cellVoltoCubic)
print("Volume of fill")
print("(first to last pass) in compaction area yd^3 : ", "%.0f" % F)


V0 = float((total_event_volume) * cellVoltoCubic)
print("Volume Under compacted                yd^3  : ", "%.0f" % V0)

# Remediation means to eliminate
VR = remediation_under_compaction_events * cellVoltoCubic
print("Remediation of under compaction       yd^3  : " , "%.0f" % -VR)

# mitigation means to reduce the effect of

VM = float((V0 - VR))
print("Remaining Volume Under compacted      yd^3  : ", "%.0f" % VM)

under_compacted_volume_percentage = VM / F
print("Percent volume under compacted              : ",  "%.0f" % ((under_compacted_volume_percentage ) * 100), "%")

double_handle_volume = double_handle_cubic * cellVoltoCubic
print("Volume of Double handle               yd^3  :" , "%.0f" % double_handle_volume), "Threshold", double_handle_threshold

C = float(total_cut * cellVoltoCubic)
print("Volume of cut  (First to last Pass)   yd^3  : ", "%.0f" % C)

V = float((total_fill + total_cut) * cellVoltoCubic)
#print("Volume of compaction area             yd^3  : ", "%.0f" % V)


print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Distribution of under compacted event >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
print("between  0 and", Machine_compaction_zone, "      : volume",  "%.0f" % (((active_volume_1 * cellVoltoCubic)/V0)*100),"%",   "%.0f" % (active_volume_1 * cellVoltoCubic), " yd^3")
print("between ", Machine_compaction_zone, "and", (Machine_compaction_zone *2), "    : volume",  "%2.0f" % (((active_volume_2 * cellVoltoCubic)/V0)*100),"%",    "%.0f" % (active_volume_2 * cellVoltoCubic), " yd^3")
print("between ", Machine_compaction_zone * 2, "and", (Machine_compaction_zone *3), "    : volume",  "%.0f" % (((active_volume_3 * cellVoltoCubic)/V0)*100),"%",    "%.0f" % (active_volume_3 * cellVoltoCubic), " yd^3")
print("between ", Machine_compaction_zone * 3, "and", (Machine_compaction_zone *4), "    : volume",  "%.0f" % (((active_volume_4 * cellVoltoCubic)/V0)*100),"%",    "%.0f" % (active_volume_4 * cellVoltoCubic), " yd^3")
print("between ", Machine_compaction_zone * 4, "and", (Machine_compaction_zone *5), "    : volume", "%.0f" % (((active_volume_5 * cellVoltoCubic)/V0)*100),"%",    "%.0f" % (active_volume_5 * cellVoltoCubic), " yd^3")
print("        ",Machine_compaction_zone * 5, "and above   : volume",  "%.0f" % (((active_volume_6 * cellVoltoCubic)/V0)*100),"%",    "%.0f" % (active_volume_6 * cellVoltoCubic), " yd^3")



print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Lift summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
print("lift passes              :", lift_passes)
#print("Less than      ", filter_elevation_uncertainty, "     :", "%.1f" % ((bin_0/lift_passes)*100) ,"%")
print("between ", filter_elevation_uncertainty, "and", Machine_compaction_zone, "    :", "%.1f" % ((bin_1/lift_passes)*100) ,"%")
print("between ", Machine_compaction_zone, "and", (Machine_compaction_zone *2), "    :",  "%.1f" % ((bin_2/lift_passes)*100) ,"%")
print("between ", Machine_compaction_zone * 2, "and", (Machine_compaction_zone *3), "    :",  "%.1f" % ((bin_3/lift_passes)*100) ,"%")
print("between ", Machine_compaction_zone * 3, "and", (Machine_compaction_zone *4), "    :",  "%.1f" % ((bin_4/lift_passes)*100) ,"%")
print("between ", Machine_compaction_zone * 4, "and", (Machine_compaction_zone *5), "    :",  "%.1f" % ((bin_5/lift_passes)*100) ,"%")
print("        ",Machine_compaction_zone * 5, "and above   :",  "%.1f" % ((bin_6/lift_passes)*100) ,"%")


print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Compaction summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
print("compaction passses       :", compaction_passes)
print("between ",  -(filter_elevation_uncertainty),  "and", -(Machine_compaction_zone),  "  :","%.1f" % ((neg_1/compaction_passes)*100) ,"%")
print("between ", -Machine_compaction_zone, "and", -(Machine_compaction_zone *2),      "  :","%.1f" % ((neg_2/compaction_passes)*100) ,"%")
print("between ", -(Machine_compaction_zone * 2), "and", -(Machine_compaction_zone *3), "  :","%.1f" % ((neg_3/compaction_passes)*100) ,"%")
print("between ", -(Machine_compaction_zone * 3), "and", -(Machine_compaction_zone *4), "  :", "%.1f" % ((neg_4/compaction_passes)*100) ,"%")
print("between ", -(Machine_compaction_zone * 4), "and", -(Machine_compaction_zone *5), "  :", "%.1f" % ((neg_5/compaction_passes)*100) ,"%")
print("        ",-(Machine_compaction_zone * 5), "and below  :", "%.1f" % ((neg_6/compaction_passes)*100) ,"%")
#print("pass check: ", cnt_Unique_cell_compaction_area - (bin_1 + bin_2 + bin_3 + bin_4 + bin_5 + bin_6 + neg_1 + neg_2 + neg_3 + neg_4 + neg_5 + neg_6))
print("over compaction passes :", total_cnt_over)
print("Percent of passes over compacting: ", "%.1f" % ((total_cnt_over/compaction_passes)*100) ,"%")


"""
print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Area of uncompacted events  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>") 
print("Volume event passes :", active_cnt, "%.4f" % (active_cnt * cellToArea), " ft^2")
print("between ", filter_low_volume_cell, "and", Machine_compaction_zone, "    : Area", "%.1f" % ((active_1/active_cnt)*100) ,"%", "%.0f" % (active_1 * cellToArea), " ft^2")
print("between ", Machine_compaction_zone, "and", (Machine_compaction_zone *2), "    : Area",  "%.1f" % ((active_2/active_cnt)*100) ,"%" , "%.0f" % (active_2 * cellToArea), " ft^2")
print("between ", Machine_compaction_zone * 2, "and", (Machine_compaction_zone *3), "    : Area",  "%.1f" % ((active_3/active_cnt)*100) ,"%", "%.0f" % (active_3 * cellToArea), " ft^2")
print("between ", Machine_compaction_zone * 3, "and", (Machine_compaction_zone *4), "    : Area",  "%.1f" % ((active_4/active_cnt)*100) ,"%", "%.0f" % (active_4 * cellToArea), " ft^2")
print("between ", Machine_compaction_zone * 4, "and", (Machine_compaction_zone *5), "    : Area",  "%.1f" % ((active_5/active_cnt)*100) ,"%", "%.0f" % (active_5 * cellToArea), " ft^2")
print("        ",Machine_compaction_zone * 5, "and above   : Area",  "%.1f" % ((active_6/active_cnt)*100) ,"%", "%.0f" % (active_6 * cellToArea), " ft^2")


print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Thick-Lift summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
print(">>>>>> Thick lift flag - count      :" , total_cnt_thick) # this is all thick lifts including repeats in single cell
print(">>>>>> Thick lift      - coverage   :", "%.4f" % (total_cnt_thick * AreatoAcre), " acres") # this is thick lift coverage, not all cells

print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Passcount details >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
print(">>>>>>>>>>>>>>>>>> (A check that this aligns with VL passcount details) >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")

passes_above_n = 1
for k,v in lst[:8]:
    print(k , "Passes " ,v, " occurences ", "%.3f" % (v/(len(northEastDict))))
    #print(cell_count_coverage)
    #print(single_pass_cells)
    #print(k, "Pass area: " ,((v * 1.41)/43560), " acres")
    passes_above_n = passes_above_n - v/(len(northEastDict))
print("Above X passes :", "%.2f" % passes_above_n)
"""



# Output file
output = "./output/CompactionOutput.csv"
fout = open(output,'a')
w = csv.writer(fout, delimiter=',',dialect='excel',lineterminator='\n')
"""
w.writerow(['Sourcefile',                           #1
            'Number machines',                      #2
            'Machine hours',                        #3
            'Total machine area',                   #4
            #'coverage area',                        #4.5
            #'Single pass',                          #5
            #'Low volume',                           #6
            #'Cut area',                             #7
            'Active area',                          #8
            'Fill',                                 #9
 #           'Cut',                                  #10
 #           'Volume Under compacted',               #11
 #           'Volume Remediated',                    #11.1
            'Undercompacted volume',                     #11.2
            'Under volume %',                       #12
           # 'Double handle',                        #13
            'Lift passes',                          #14
            'Lift 0 to 0.5',                        #15
            'Lift 0.5 to 1.0',                      #16
            'Lift 1.0 to 1.5',                      #17
            'Lift 1.5 to 2.0',                      #18
            'Lift 2.0 to 2.5',                      #19
            'Lift > 2.5',                           #20
            'Comp passes' ,                          #21
           # 'Comp 0 to -0.5',                       #22
           # 'Comp -0.5 to -1.0',                    #23
           # 'Comp -1.0 to -1.5',                    #24
           # 'Comp -1.5 to -2.0',                    #25
           # 'Comp -2.0 to -2.5',                    #26
           # 'Comp < -2.5',                          #27
            '% Over Compaction',                      #28
            'Event 0 to 0.5 volume',                       #29
            'Event 0.5 to 1.0 volume',                     #30
            'Event 1.0 to 1.5 volume',                     #31
            'Event 1.5 to 2.0 volume',                     #32
            'Event 2.0 to 2.5 volume',                     #33
            'Event > 2.5 volume',                          #34
            ])
            """
      
w.writerow([current_file,                                               #1
            len(contributing_machines),                                 #2
            total_duration,                                             #3
            "%.4f" % (cell_pass_count_total * AreatoAcre),              #4                        
           # "%.4f" % (area_coverage) ,   #4.5
           # "%.4f" %  (single_pass_area) ,                              #5
           # "%.4f" % (area_low_volume_cell) ,                           #6
           # "%.4f" % (area_cut_cell) ,                                  #7
            "%.4f" % (active_compaction_area) ,                         #8
            "%.0f" % F,                                                #9
           # "%.0f" % -C,                                               #10
           # "%.0f" % V0,                                                #11
           # "%.0f" % VR,                                                #11.1
            "%.0f" % VM,                                                #11.2
            "%.0f" % ((under_compacted_volume_percentage ) * 100),     #12
           # "%.0f" % (double_handle_volume),                              #13
            lift_passes  ,                                                  #14
            "%.1f" % ((bin_1/lift_passes)*100) ,  
            "%.1f" % ((bin_2/lift_passes)*100) , 
            "%.1f" % ((bin_3/lift_passes)*100) , 
            "%.1f" % ((bin_4/lift_passes)*100) , 
            "%.1f" % ((bin_5/lift_passes)*100) , 
            "%.1f" % ((bin_6/lift_passes)*100) , 
            compaction_passes,                                          #21
           # "%.1f" % ((neg_1/compaction_passes)*100) ,
           # "%.1f" % ((neg_2/compaction_passes)*100) ,
           # "%.1f" % ((neg_3/compaction_passes)*100) ,
           # "%.1f" % ((neg_3/compaction_passes)*100) ,
           # "%.1f" % ((neg_5/compaction_passes)*100) ,
           # "%.1f" % ((neg_6/compaction_passes)*100) ,
            "%.1f" % ((total_cnt_over/compaction_passes)*100) ,          #28
            "%.0f" % (active_volume_1 * cellVoltoCubic),
            "%.0f" % (active_volume_2 * cellVoltoCubic),
            "%.0f" % (active_volume_3 * cellVoltoCubic),
            "%.0f" % (active_volume_4 * cellVoltoCubic),
            "%.0f" % (active_volume_5 * cellVoltoCubic),
            "%.0f" % (active_volume_6 * cellVoltoCubic),     #34

            ])
fout.close()

#print(machine_area_eventDict)

# prepare Dictionary for Excel output of undercompacted material distribution

# prepare site summary for Excel
site_summary_to_excel = (
    ['file processed', filename],
    ['Thicklift value', thick_lift],
    ['Low volume cell filter', filter_low_volume_cell],
    ['uncertainty filter',filter_elevation_uncertainty],
    ['Machine compaction zone', Machine_compaction_zone],
    ['Contributing machines',len(contributing_machines)],
    ['total hours',total_duration],
    ['Machine Operation Coverage area Acres',(cell_pass_count_total * AreatoAcre)],
    ['Coverage - Area(VL uses) Acres',area_coverage],
    ['Removed for single pass Acres',single_pass_area],
    ['Removed for Cut Acres',area_cut_cell],
    ['Removed for low volume Acres',area_low_volume_cell],
    ['Active compaction - area Acres',active_compaction_area],
    ['Volume of Fill yd^3',F],
    ['Total Volume under compacted yd^3',V0],
    ['Remediated under compacted yd^3',-VR],
    ['Remaining undercomapcted yd^3',VM],
    ['% volume under compacted',((under_compacted_volume_percentage ) * 100)],
    ['volume of double handle',double_handle_volume],
    ['volume of cut',-C],
    ['under compacted event % 0 to 0.5',(((active_volume_1 * cellVoltoCubic)/V0)*100)],
    ['under compacted event % 0.5 to 1.0',(((active_volume_2 * cellVoltoCubic)/V0)*100)],
    ['under compacted event % 1.0 to 1.5',(((active_volume_3 * cellVoltoCubic)/V0)*100)],
    ['under compacted event % 1.5 to 2.0',(((active_volume_4 * cellVoltoCubic)/V0)*100)],
    ['under compacted event % 2.0 to 2.5',(((active_volume_5 * cellVoltoCubic)/V0)*100)],
    ['under compacted event % 2.5 >',(((active_volume_6 * cellVoltoCubic)/V0)*100)],
    ['under compacted event volume 0 to 0.5',(active_volume_1 * cellVoltoCubic)],
    ['under compacted event volume 0.5 to 1.0',(active_volume_2 * cellVoltoCubic)],
    ['under compacted event volume 1.0 to 1.5',(active_volume_3 * cellVoltoCubic)],
    ['under compacted event volume 1.5 to 2.0',(active_volume_4 * cellVoltoCubic)],
    ['under compacted event volume 2.0 to 2.5',(active_volume_5 * cellVoltoCubic)],
    ['under compacted event volume 2.5 >',(active_volume_6 * cellVoltoCubic)],
    ['lift passes', lift_passes],
    ['compaction passes', compaction_passes],
    ['lift between filter and 0.5',((bin_1/lift_passes)*100)],
    ['lift between 0.5 and 1.0',((bin_2/lift_passes)*100)],
    ['lift between 1.0 and 1.5',((bin_3/lift_passes)*100)],
    ['lift between 1.5 and 2.0',((bin_4/lift_passes)*100)],
    ['lift between 2.0 and 2.5',((bin_5/lift_passes)*100)],
    ['lift greater than 2.5',((bin_6/lift_passes)*100)],

    
)

#create dictionary to display lift sumary by %
lift_summaryDict = {"Lift Summary breakdown" : {}, }
lift_summary_breakdown_all ={}
ucv = ((bin_1/lift_passes)*100)
lift_summary_breakdown_all['0 to 0.5'] = ucv
ucv = ((bin_2/lift_passes)*100)
lift_summary_breakdown_all['0.5 to 1.0'] = ucv
ucv = ((bin_3/lift_passes)*100)
lift_summary_breakdown_all['1.0 to 1.5'] = ucv
ucv = ((bin_4/lift_passes)*100)
lift_summary_breakdown_all['1.5 to 2.0'] = ucv
ucv = ((bin_5/lift_passes)*100)
lift_summary_breakdown_all['2.0 to 2.5'] = ucv
ucv = ((bin_6/lift_passes)*100)
lift_summary_breakdown_all['> 2.5'] = ucv

lift_summaryDict['Lift Summary breakdown'] = [lift_summary_breakdown_all]

# create dictionary to display undercompacted material magnitude breakdown by %
under_compacted_material_magnitude_breakdownDict = {"Under compacted breakdown" : {}, }
under_compacted_breakdown_all ={}
ucv = (active_volume_1 * cellVoltoCubic)
under_compacted_breakdown_all['0 to 0.5'] = ucv
ucv = (active_volume_2 * cellVoltoCubic)
under_compacted_breakdown_all['0.5 to 1.0'] = ucv
ucv = (active_volume_3 * cellVoltoCubic)
under_compacted_breakdown_all['1.0 to 1.5'] = ucv
ucv = (active_volume_4 * cellVoltoCubic)
under_compacted_breakdown_all['1.5 to 2.0'] = ucv
ucv = (active_volume_5 * cellVoltoCubic)
under_compacted_breakdown_all['2.0 to 2.5'] = ucv
ucv = (active_volume_6 * cellVoltoCubic)
under_compacted_breakdown_all['> 2.5'] = ucv

under_compacted_material_magnitude_breakdownDict['Under compacted breakdown'] = [under_compacted_breakdown_all]

# create dictionary to display undercompacted material volume by magnitude
under_compacted_volume_breakdownDict = {"Under compacted vol breakdown" : {}, }
under_compacted_volume_breakdown_all ={}
ucv = int((((active_volume_1 * cellVoltoCubic)/VM)*100))
under_compacted_volume_breakdown_all['0 to 0.5'] = ucv
ucv = int((((active_volume_2 * cellVoltoCubic)/VM)*100))
under_compacted_volume_breakdown_all['0.5 to 1.0'] = ucv
ucv = int((((active_volume_3 * cellVoltoCubic)/VM)*100))
under_compacted_volume_breakdown_all['1.0 to 1.5'] = ucv
ucv = int((((active_volume_4 * cellVoltoCubic)/VM)*100))
under_compacted_volume_breakdown_all['1.5 to 2.0'] = ucv
ucv = int((((active_volume_5* cellVoltoCubic)/VM)*100))
under_compacted_volume_breakdown_all['2.0 to 2.5'] = ucv
ucv = int((((active_volume_6 * cellVoltoCubic)/VM)*100))
under_compacted_volume_breakdown_all['> 2.5'] = ucv

under_compacted_volume_breakdownDict['Under compacted vol breakdown'] = [under_compacted_volume_breakdown_all]

##########################################################
# create Volume dictionary and write to .xlxs file
##########################################################
ExcelOutputFile = "./output/" + current_file +".xlsx"

tempVolumeDict = {} # mitigated volume
tempVolumeDict_accumulated = {}

for machine, dk in machine_volume_eventDict.items():
    accumulated_vol = 0.0   
    for times in dk:
        lst = list(times.keys())
        lst.sort()
        for key in lst:
            vol = float(times[key] * cellVoltoCubic)
            accumulated_vol = accumulated_vol + vol
            vol = round(vol,0)
            accumulated_vol = round(accumulated_vol,0)
            tempVolumeDict.setdefault(machine,{})[key] = vol
            tempVolumeDict_accumulated.setdefault(machine,{})[key] = accumulated_vol

df1 = pd.DataFrame(tempVolumeDict)
df100 = pd.DataFrame(tempVolumeDict_accumulated)
#print(tempVolumeDict_accumulated)


tempVolumeDict = {} # undercompacted material breakdown
#print(under_compacted_material_magnitude_breakdownDict)
for machine, dk in under_compacted_material_magnitude_breakdownDict.items():
    
    for times in dk:
        lst = list(times.keys())
        lst.sort()
        for key in lst:
            vol = int(times[key])
            #print(vol)
            #vol = round(vol,0)
            tempVolumeDict.setdefault(machine,{})[key] = vol
df5 = pd.DataFrame(tempVolumeDict)

tempVolumeDict = {} # undercompacted material breakdown
#print(under_compacted_material_magnitude_breakdownDict)
for machine, dk in under_compacted_volume_breakdownDict.items():
    
    for times in dk:
        lst = list(times.keys())
        lst.sort()
        for key in lst:
            vol = int(times[key])
            #print(vol)
            #vol = round(vol,0)
            tempVolumeDict.setdefault(machine,{})[key] = vol
df6 = pd.DataFrame(tempVolumeDict)

#Lift Summary breakdown
tempVolumeDict = {} # Lift Summary breakdown
#print(under_compacted_material_magnitude_breakdownDict)
for machine, dk in lift_summaryDict.items():
    
    for times in dk:
        lst = list(times.keys())
        lst.sort()
        for key in lst:
            vol = int(times[key])
            #print(vol)
            #vol = round(vol,0)
            tempVolumeDict.setdefault(machine,{})[key] = vol
df7 = pd.DataFrame(tempVolumeDict)

tempVolumeDict = {} # unmitigated volume
for machine, dk in machine_volume_eventDict_positive.items():
    
    for times in dk:
        lst = list(times.keys())
        lst.sort()
        for key in lst:
            vol = float(times[key] * cellVoltoCubic)
            #vol = round(vol,0)
            tempVolumeDict.setdefault(machine,{})[key] = vol
df2 = pd.DataFrame(tempVolumeDict)

tempVolumeDict = {} # remediated volume
tempVolumeDict_accumulated = {}
for machine, dk in machine_volume_eventDict_negative.items():
    accumulated_remediation = 0.0 
    for times in dk:
        lst = list(times.keys())
        lst.sort()    
        for key in lst:
            vol = float(times[key] * cellVoltoCubic)
            vol = round(vol,0)
            accumulated_remediation = accumulated_remediation + vol
            accumulated_remediation = round(accumulated_remediation)
            tempVolumeDict.setdefault(machine,{})[key] = vol
            tempVolumeDict_accumulated.setdefault(machine,{})[key] = accumulated_remediation
df3 = pd.DataFrame(tempVolumeDict)
df300 = pd.DataFrame(tempVolumeDict_accumulated)

tempAreaDict = {}
for machine, dk in machine_area_eventDict.items():    
    for times in dk:
        lst = list(times.keys())
        #print("max row",max_row)
        lst.sort()
        for key in lst:
            sqft = float(times[key] * cellToArea)
            sqft = round(sqft,0)
            tempAreaDict.setdefault(machine,{})[key] = sqft

df10 = pd.DataFrame(tempAreaDict)


# create sheets
writer = pd.ExcelWriter(ExcelOutputFile, engine='xlsxwriter')
workbook  = writer.book
worksheetSummary = workbook.add_worksheet('Site summary')
df1.to_excel(writer,'Mitigated_Volume')
df100.to_excel(writer,'Mitigated Volume accum')
#df2.to_excel(writer,'(+) vol evnt')
df3.to_excel(writer,'(-) vol evnt')
df300.to_excel(writer,'(-) vol evnt accum')
df5.to_excel(writer,'undercompacted volume breakdown')
#df6.to_excel(writer,'undercompacted event breakdown')
df7.to_excel(writer,'lift summary breakdown')
#df10.to_excel(writer,'Area Events')


###############################################
# Site summary information
row = 0
col = 0
for item, value in (site_summary_to_excel):
    worksheetSummary.write(row, col, item)
    worksheetSummary.write(row,col +1, value)
    row +=1

max_row = 13
###############################################
# df100 create chart for mitigated volume acumulation
worksheet100 = writer.sheets['Mitigated Volume accum']
chart = workbook.add_chart({'type': 'line'})
for i in range(len(machine_volume_eventDict.items())):
    col = i + 1
    chart.add_series({
        'name':       ['Mitigated Volume accum', 0, col],
        'categories': ['Mitigated Volume accum', 1, 0,max_row, 0],
        'values':     ['Mitigated Volume accum', 1, col, max_row, col],  
    })
# Configure the chart axes.
chart.set_x_axis({'name': 'Hour of Day'})
chart.set_y_axis({'name': 'Volume', 'major_gridlines': {'visible': True}})
chart.set_title({'name': 'Undercompacted Volume - Accumulated'})
chart.set_size({'x_scale': 2.25, 'y_scale': 3})
# Insert the chart into the worksheet.
worksheet100.insert_chart('G2', chart)
# End create chart for mitigated volume acumulation
###############################################
###############################################
# df1 create chart for mitigated volume
worksheet1 = writer.sheets['Mitigated_Volume']
chart = workbook.add_chart({'type': 'line'})
for i in range(len(machine_volume_eventDict.items())):
    col = i + 1
    chart.add_series({
        'name':       ['Mitigated_Volume', 0, col],
        'categories': ['Mitigated_Volume', 1, 0,max_row, 0],
        'values':     ['Mitigated_Volume', 1, col, max_row, col],  
    })
# Configure the chart axes.
chart.set_x_axis({'name': 'Hour of Day'})
chart.set_y_axis({'name': 'Volume', 'major_gridlines': {'visible': True}})
chart.set_title({'name': ' Undercompacted Volume - Hour of day'})
chart.set_size({'x_scale': 2.25, 'y_scale': 3})
# Insert the chart into the worksheet.
worksheet1.insert_chart('G2', chart)
# End create chart for mitigated volume
###############################################
###############################################
# df3 remediation events
worksheet3 = writer.sheets['(-) vol evnt']
chart = workbook.add_chart({'type': 'line'})
for i in range(len(machine_volume_eventDict.items())):
    col = i + 1
    chart.add_series({
        'name':       ['(-) vol evnt', 0, col],
        'categories': ['(-) vol evnt', 1, 0,max_row, 0],
        'values':     ['(-) vol evnt', 1, col, max_row, col],  
    })
# Configure the chart axes.
chart.set_x_axis({'name': 'Hour of Day'})
chart.set_y_axis({'name': 'Volume', 'major_gridlines': {'visible': True}})
chart.set_title({'name': 'Undercompacted volume removed - Hour of day'})
chart.set_size({'x_scale': 2.25, 'y_scale': 3})
# Insert the chart into the worksheet.
worksheet3.insert_chart('G2', chart)
# df3 remediation events
###############################################
###############################################
# df300 accumulated remediation
worksheet300 = writer.sheets['(-) vol evnt accum']
chart = workbook.add_chart({'type': 'line'})
for i in range(len(machine_volume_eventDict.items())):
    col = i + 1
    chart.add_series({
        'name':       ['(-) vol evnt accum', 0, col],
        'categories': ['(-) vol evnt accum', 1, 0,max_row, 0],
        'values':     ['(-) vol evnt accum', 1, col, max_row, col],  
    })
# Configure the chart axes.
chart.set_x_axis({'name': 'Hour of Day'})
chart.set_y_axis({'name': 'Volume', 'major_gridlines': {'visible': True}})
chart.set_title({'name': 'Undercompacted volume removed - Accumulated'})
chart.set_size({'x_scale': 2.25, 'y_scale': 3})
# Insert the chart into the worksheet.
worksheet300.insert_chart('G2', chart)
# End df300 accumulated remediation
###############################################
###############################################

# df6 breakdown event magnitude by volume
worksheet6 = writer.sheets['undercompacted volume breakdown']
chart = workbook.add_chart({'type': 'column'})
max_row = 6
for i in range(len(machine_volume_eventDict.items())):
    col = i + 1
    chart.add_series({
        'name':       ['undercompacted volume breakdown', 0, col],
        'categories': ['undercompacted volume breakdown', 1, 0,max_row, 0],
        'values':     ['undercompacted volume breakdown', 1, col, max_row, col],  
    })
# Configure the chart axes.
chart.set_x_axis({'name': 'Magnitude of event'})
chart.set_y_axis({'name': 'Volume', 'major_gridlines': {'visible': True}})
chart.set_title({'name': 'Undercompacted event volume breakdown'})
chart.set_size({'x_scale': 2.25, 'y_scale': 3})
# Insert the chart into the worksheet.
worksheet6.insert_chart('G2', chart)
# End create chart for mitigated volume acumulation

###############################################
###############################################
# df7 lift breakdown
worksheet7 = writer.sheets['lift summary breakdown']
chart = workbook.add_chart({'type': 'column'})
max_row = 6
for i in range(len(lift_summaryDict.items())):
    col = i + 1
    chart.add_series({
        'name':       ['lift summary breakdown', 0, col],
        'categories': ['lift summary breakdown', 1, 0,max_row, 0],
        'values':     ['lift summary breakdown', 1, col, max_row, col],  
    })
# Configure the chart axes.
chart.set_x_axis({'name': 'Magnitude of Lift'})
chart.set_y_axis({'name': 'percentage of lifts', 'major_gridlines': {'visible': True}})
chart.set_title({'name': 'Lift summary breakdown by %'})
chart.set_size({'x_scale': 2.25, 'y_scale': 3})
# Insert the chart into the worksheet.
worksheet7.insert_chart('G2', chart)
# End lift breakdown
###############################################
###############################################
"""
# df5 breakdown of undercompacted events
worksheet5 = writer.sheets['undercompacted event breakdown']
chart = workbook.add_chart({'type': 'column'})
max_row = 6
for i in range(len(under_compacted_material_magnitude_breakdownDict.items())):
    col = i + 1
    chart.add_series({
        'name':       ['undercompacted event breakdown', 0, col],
        'categories': ['undercompacted event breakdown', 1, 0,max_row, 0],
        'values':     ['undercompacted event breakdown', 1, col, max_row, col],  
    })
# Configure the chart axes.
chart.set_x_axis({'name': 'Magnitude of events'})
chart.set_y_axis({'name': 'Percentage of Volume', 'major_gridlines': {'visible': True}})
chart.set_title({'name': 'under comapcted event magnitude breakdown %'})
chart.set_size({'x_scale': 2.25, 'y_scale': 3})
# Insert the chart into the worksheet.
worksheet5.insert_chart('G2', chart)
# End create chart for mitigated volume acumulation
"""
###############################################
writer.save()

#print("wrote data to {}".format(outputEvent))