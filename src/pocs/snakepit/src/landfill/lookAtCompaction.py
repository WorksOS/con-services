import codecs
from datetime import datetime
import csv
import sys
import pandas as pd
from typing import Dict, Tuple, List
from src.landfill.compaction_evaulation import CompactionEvaluation
import xlsxwriter
# from pandas import ExcelWriter
# from openpyxl import Workbook

from pathlib import Path


class LandfillAlgorithm():

    US_STANDARD_HEADER_SUFFIX = "_FT"



    def __init__(self, metric=True, create_points=False):
        self.create_points = create_points
        self.is_metric = metric

        # Set Variables - SI or Metric
        self.machine_compaction_zone = 0.5  # for an 836
        self.cell_to_area = 1.244  # A cell is 34 cm squared
        self.AreatoAcre = self.cell_to_area / 43560
        self.cellVoltoCubic = self.cell_to_area / 27
        self.double_handle_threshold = -2.0  # used to define rework based on vertical change down
        self.thick_lift_threshold = 2.5  # used to measure how often too much material is placed
        self.filter_low_volume_cell = 0.0  # used to refine area of operation by removing cells with little volume
        self.filter_elevation_uncertainty = 0.0  # used to keep passes with little elevation change out of compaction and lift statistics
        self.current_file = None


    '''Set the units used depending on what is in the file)'''
    def set_units(self):
        #TODO These conversions look suspicious, needs refactoring all calcs should be in SI units and converted at presentation
        if self.is_metric:
            self.machine_compaction_zone = 0.5  # for an 836
            self.cell_to_area = 1.244
            self.AreatoAcre = self.cell_to_area / 43560
            self.cellVoltoCubic = self.cell_to_area / 27
            self.double_handle_threshold = 3.0
            self.thick_lift_threshold = 2.5
            self.filter_low_volume_cell = 0.5



    def generate_report(self, input_file, output_location=None):

        filename = input_file
        self.current_file = Path(filename).stem
        cell_pass_count_total, north_east_dict = self.build_ne_dict(filename)

        #do we want/need point cloud? Probably not but here it is just in case

        #analyse_machine_operation

        #generate cell summary
        bin_pass_counts, cell_summaries = self.generate_cell_summary(north_east_dict)




    #TODO sort out cell header names depending on type
    '''Read a cav file like object and return a tuple with passcount and NE Dict '''
    def build_ne_dict(self, filename: str) -> Tuple[int, Dict]:
        #############################################################
        # read CSV file into dictionary grouped by unique NE values #
        #############################################################
        northEastDict = {}
        cell_pass_count_total = 0

        with codecs.open(filename, mode="r", encoding="utf-8-sig") as csvFile:

            for csvRow in csv.DictReader(csvFile):
                cell_pass_count_total = cell_pass_count_total + 1  # total number of cell passes in the file
                northEastData = {
                    "dateTime": datetime.strptime(csvRow["Time"], "%Y/%b/%d %H:%M:%S.%f"),
                    "machine": csvRow["Machine"],
                    "elevation": float(csvRow["Elevation_FT"]),
                    "passNumber": int(csvRow["PassNumber"]),
                    "delta_from_last_pass": 0.0,
                    "EventMagnitude:": 0.0,
                }
                northEast = (float(csvRow["CellN_FT"]), float(csvRow["CellE_FT"]))
                northEastDict.setdefault(northEast, []).append(northEastData)
            # End of for csvRow in csvReader:
        # End of with open(filename) as csvFile:
        # created a dictionary grouped by NE grid
        return cell_pass_count_total, northEastDict

    '''Get set of machines which have contriubted to the project '''
    def get_contributing_machines(self, northEastDict):

        contributing_machines = set()
        # find contributing machines
        for cell, pass_list in northEastDict.items():
            for cell_pass in pass_list:
                # find which machines worked in the time period
                contributing_machines.add(cell_pass['machine'])

        return contributing_machines

    '''Determine machine operation time (this is quite likely to be inaccurate) '''
    def analyse_machine_operation(self, machines_to_analyse, northEastDict) -> Tuple[float, Dict]:
        #TODO Really check this logic - I not sure we have the right info here to determine this in a meaningful way, additionally the calc looks wrong

        total_duration = 0.0
        machine_duration = 0.0
        machine_operation_dict = {}

        for machine in machines_to_analyse:
            start_time = None
            stop_time = None
            start_hour = None
            # Find start and stop time for each machine
            for k, dk in northEastDict.items():
                for x in dk:  # x is the ? # for all keys
                    if machine == x['machine']:
                        DateTimeSTR = str(x['dateTime'])
                        #sppos = DateTimeSTR.find(' ')
                        current_time = DateTimeSTR[10 + 1:]
                        #current_date = DateTimeSTR[:10]
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
                        duration_minute = (int(stop_minute) - int(start_minute)) / 60
                        machine_duration = duration_hour + duration_minute
            machine_operation_data = {
                "duration": machine_duration,
                "start": start_time,
                "stop": stop_time,
            }
            machine_operation_dict.setdefault(machine, []).append(machine_operation_data)
            total_duration = float(total_duration) + float(machine_duration)
        return total_duration, machine_operation_dict

    def generate_point_cloud_surfaces(self, northEastDict):
        top_surface = []
        bottom_surface = []

        for cell, passes in northEastDict.items():
            first_elevation = None
            last_elevation = None
            max_pass_count = None

            for cell_pass in passes:  # find first and last pass elevations to calculate cell volume (cut or fill)
                if first_elevation is None or int(cell_pass['passNumber']) == 1:
                    first_elevation = float(cell_pass['elevation'])

                if max_pass_count is None or int(max_pass_count) <= int(cell_pass['passNumber']):
                    max_pass_count = cell_pass['passNumber']
                    last_elevation = float(cell_pass['elevation'])

            top_surface.append([cell[1], cell[0], last_elevation])
            bottom_surface.append([cell[1], cell[0], first_elevation])

    def output_csv(self, filename: str, data: List):
        with open(filename, "w") as out_file:
            csv_out = csv.writer(out_file, delimiter=',', dialect='excel', lineterminator='\n')
            for row in data:
                csv_out.write(row)

    def generate_cell_summary(self, northEastDict) -> Tuple[Dict, Dict]:
        ############################
        # CELL SUMMARY INFORMATION #
        ############################
        bin_pass_counts = {}  # This is to calc 1 pass cells and to compare passcounts to VL
        cell_summaries = {}

        ################################
        # Find the volume of each cell and add summary dictionary to cells with more than one pass #
        ################################
        for cell, passes in northEastDict.items():
            first_elevation = None
            last_elevation = None
            max_pass_count = None
            cell_cut_volume = 0.0
            cell_fill_volume = 0.0

            for cell_pass in passes:  # find first and last pass elevations to calculate cell volume (cut or fill)
                if first_elevation is None or cell_pass['passNumber'] == 1:
                    first_elevation = cell_pass['elevation']

                if max_pass_count is None or max_pass_count <= cell_pass['passNumber']:
                    max_pass_count = cell_pass['passNumber']
                    last_elevation = cell_pass['elevation']

            cell_volume = last_elevation - first_elevation

            # separate into cut/fill and total
            if cell_volume < self.filter_low_volume_cell * -1:
                cell_cut_volume = cell_volume

            if cell_volume > self.filter_low_volume_cell:
                cell_fill_volume = cell_volume

            # add summary dictionary and insert cut/fill, volume and maximum passcount informaion
            if max_pass_count > 1:  # only add summary information to cells with more than one pass
                summaryData = {
                    "passes_for_cell": int(max_pass_count),
                    "volume": cell_volume,
                    "cut": cell_cut_volume,
                    "fill": cell_fill_volume,
                    "volume_event": 0.0,
                    "cnt_over_compaction": 0,
                    "cnt_self.thick_lift": 0,
                    "cnt_under_compaction": 0,
                    "air_space_utilization": 0.0,
                    "efficiency_index": 0.0,
                }

                # append summary information to current NE key in northEastDict dictionary
                #northEastDict[cell].append(summaryData)
                cell_summaries[cell] = summaryData

            # bin passcount occurances into a dictionary (to compare with VL and to tally single pass cells)
            bin_pass_counts[max_pass_count] = bin_pass_counts.get(max_pass_count, 0) + 1

        return bin_pass_counts, cell_summaries




    ''' Not currently used
    def print_compaction_report(self):
        #####################################################################################################################################
        # Begin writing output
        #####################################################################################################################################
        print(
            "**********************************************************************************************************************************************")
        print("file processed: ", self.filename)
        print("Variables set :", self.thick_lift, "Thick lift threshold ")
        print("Variables set :", self.filter_low_volume_cell, "Minimum volume filter")
        print("Variables set :", self.machine_compaction_zone, "Compaction ratio ")
        print("Cell to sq ft conversion 1.244")
        print(
            "**********************************************************************************************************************************************")
        print("Machines contributing :", len(contributing_machines))
        # for v in contributing_machines:
        #    print(v)
        # print("Start Time : ",start_time)
        # print("Stop  Time : ",stop_time)
        # print("%.3f" % machine_duration, "hours")
        print("%.2f" % total_duration, "hours")

        for cell, passes in machine_operation_dict.items():
            for x in passes:
                print(cell, "start time :", x['start'], "stop time :", x['stop'], "duration :", "%.2f" % x['duration'])

        print(
            ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Area - Site summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
        print("Machine Operation Coverage area   :", "%.4f" % (cell_pass_count_total * self.AreatoAcre), " acres")
        area_coverage = (len(northEastDict) * self.AreatoAcre)
        print("Coverage - Area(VL uses)          :", "%.4f" % (area_coverage), " acres")
        single_pass_area = single_pass_cells * self.AreatoAcre
        print("Removed for single pass           :", "%.4f" % (single_pass_area), " acres")
        area_cut_cell = cnt_cut_cells * self.AreatoAcre
        print("Removed for Cut                   :", "%.4f" % (area_cut_cell), " acres")
        print("Variables set :", self.filter_low_volume_cell, "Minimum volume filter")
        area_low_volume_cell = cnt_low_volume_cells * self.AreatoAcre
        print("Removed for low volume            :", "%.4f" % (area_low_volume_cell), " acres")
        active_compaction_area = (cnt_unique_cell_compaction_area * self.AreatoAcre)
        print("Active compaction - area          :", "%.4f" % (active_compaction_area), " acres")
        # print("Area check                        :", (area_coverage - single_pass_area - area_low_volume_cell - area_cut_cell))

        print(
            ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Passes - Site summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
        print("Total pass                        :", cell_pass_count_total)
        print("Removed for single pass           :", single_pass_cells)
        print("Removed for low volume            :", cnt_low_volume_passes)
        print("Removed for Cut                   :", cnt_cut_passes)
        print("Positive                          :", positive_passes)
        print("Negative                          :", negative_passes)
        # print("check : ",(cell_pass_count_total - single_pass_cells - cnt_low_volume_passes - cnt_cut_passes  - positive_passes - negative_passes))
        print("Variables set :", self.filter_elevation_uncertainty, " elevation_uncertainty")
        print("filter lift                       :", cnt_lift_uncertainty)
        print("filter compaction                 :", cnt_compact_uncertainty)
        lift_passes = positive_passes - cnt_lift_uncertainty
        print("lift                              :", lift_passes)
        compaction_passes = negative_passes - cnt_compact_uncertainty
        print("Compaction                        :", compaction_passes)
        print("Pass check                        :")

        print(
            ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Airspace Performance summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")

        F = float(total_fill * self.cellVoltoCubic)
        print("Volume of fill")
        print("(first to last pass) in compaction area yd^3 : ", "%.0f" % F)

        V0 = float((total_event_volume) * self.cellVoltoCubic)
        print("Volume Under compacted                yd^3  : ", "%.0f" % V0)

        # Remediation means to eliminate
        VR = remediation_under_compaction_events * self.cellVoltoCubic
        print("Remediation of under compaction       yd^3  : ", "%.0f" % -VR)

        # mitigation means to reduce the effect of

        VM = float((V0 - VR))
        print("Remaining Volume Under compacted      yd^3  : ", "%.0f" % VM)

        under_compacted_volume_percentage = VM / F
        print("Percent volume under compacted              : ", "%.0f" % ((under_compacted_volume_percentage) * 100),
              "%")

        double_handle_volume = double_handle_cubic * self.cellVoltoCubic
        print("Volume of Double handle               yd^3  :",
              "%.0f" % double_handle_volume), "Threshold", self.double_handle_threshold

        C = float(total_cut * self.cellVoltoCubic)
        print("Volume of cut  (First to last Pass)   yd^3  : ", "%.0f" % C)

        V = float((total_fill + total_cut) * self.cellVoltoCubic)
        # print("Volume of compaction area             yd^3  : ", "%.0f" % V)

        print(
            ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Distribution of under compacted event >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
        print("between  0 and", self.machine_compaction_zone, "      : volume",
              "%.0f" % (((active_volume_1 * self.cellVoltoCubic) / V0) * 100), "%",
              "%.0f" % (active_volume_1 * self.cellVoltoCubic), " yd^3")
        print("between ", self.machine_compaction_zone, "and", (self.machine_compaction_zone * 2), "    : volume",
              "%2.0f" % (((active_volume_2 * self.cellVoltoCubic) / V0) * 100), "%",
              "%.0f" % (active_volume_2 * self.cellVoltoCubic), " yd^3")
        print("between ", self.machine_compaction_zone * 2, "and", (self.machine_compaction_zone * 3), "    : volume",
              "%.0f" % (((active_volume_3 * self.cellVoltoCubic) / V0) * 100), "%",
              "%.0f" % (active_volume_3 * self.cellVoltoCubic), " yd^3")
        print("between ", self.machine_compaction_zone * 3, "and", (self.machine_compaction_zone * 4), "    : volume",
              "%.0f" % (((active_volume_4 * self.cellVoltoCubic) / V0) * 100), "%",
              "%.0f" % (active_volume_4 * self.cellVoltoCubic), " yd^3")
        print("between ", self.machine_compaction_zone * 4, "and", (self.machine_compaction_zone * 5), "    : volume",
              "%.0f" % (((active_volume_5 * self.cellVoltoCubic) / V0) * 100), "%",
              "%.0f" % (active_volume_5 * self.cellVoltoCubic), " yd^3")
        print("        ", self.machine_compaction_zone * 5, "and above   : volume",
              "%.0f" % (((active_volume_6 * self.cellVoltoCubic) / V0) * 100), "%",
              "%.0f" % (active_volume_6 * self.cellVoltoCubic), " yd^3")

        print(
            ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Lift summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
        print("lift passes              :", lift_passes)
        # print("Less than      ", self.filter_elevation_uncertainty, "     :", "%.1f" % ((bin_0/lift_passes)*100) ,"%")
        print("between ", self.filter_elevation_uncertainty, "and", self.machine_compaction_zone, "    :",
              "%.1f" % ((bin_1 / lift_passes) * 100), "%")
        print("between ", self.machine_compaction_zone, "and", (self.machine_compaction_zone * 2), "    :",
              "%.1f" % ((bin_2 / lift_passes) * 100), "%")
        print("between ", self.machine_compaction_zone * 2, "and", (self.machine_compaction_zone * 3), "    :",
              "%.1f" % ((bin_3 / lift_passes) * 100), "%")
        print("between ", self.machine_compaction_zone * 3, "and", (self.machine_compaction_zone * 4), "    :",
              "%.1f" % ((bin_4 / lift_passes) * 100), "%")
        print("between ", self.machine_compaction_zone * 4, "and", (self.machine_compaction_zone * 5), "    :",
              "%.1f" % ((bin_5 / lift_passes) * 100), "%")
        print("        ", self.machine_compaction_zone * 5, "and above   :", "%.1f" % ((bin_6 / lift_passes) * 100),
              "%")

        print(
            ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Compaction summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
        print("compaction passses       :", compaction_passes)
        print("between ", -(self.filter_elevation_uncertainty), "and", -(self.machine_compaction_zone), "  :",
              "%.1f" % ((neg_1 / compaction_passes) * 100), "%")
        print("between ", -self.machine_compaction_zone, "and", -(self.machine_compaction_zone * 2), "  :",
              "%.1f" % ((neg_2 / compaction_passes) * 100), "%")
        print("between ", -(self.machine_compaction_zone * 2), "and", -(self.machine_compaction_zone * 3), "  :",
              "%.1f" % ((neg_3 / compaction_passes) * 100), "%")
        print("between ", -(self.machine_compaction_zone * 3), "and", -(self.machine_compaction_zone * 4), "  :",
              "%.1f" % ((neg_4 / compaction_passes) * 100), "%")
        print("between ", -(self.machine_compaction_zone * 4), "and", -(self.machine_compaction_zone * 5), "  :",
              "%.1f" % ((neg_5 / compaction_passes) * 100), "%")
        print("        ", -(self.machine_compaction_zone * 5), "and below  :",
              "%.1f" % ((neg_6 / compaction_passes) * 100), "%")
        # print("pass check: ", cnt_Unique_cell_compaction_area - (bin_1 + bin_2 + bin_3 + bin_4 + bin_5 + bin_6 + neg_1 + neg_2 + neg_3 + neg_4 + neg_5 + neg_6))
        print("over compaction passes :", total_cnt_over)
        print("Percent of passes over compacting: ", "%.1f" % ((total_cnt_over / compaction_passes) * 100), "%")

        """
        print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Area of uncompacted events  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>") 
        print("Volume event passes :", active_cnt, "%.4f" % (active_cnt * self.cellToArea), " ft^2")
        print("between ", self.filter_low_volume_cell, "and", self.machine_compaction_zone, "    : Area", "%.1f" % ((active_1/active_cnt)*100) ,"%", "%.0f" % (active_1 * self.cellToArea), " ft^2")
        print("between ", self.machine_compaction_zone, "and", (self.machine_compaction_zone *2), "    : Area",  "%.1f" % ((active_2/active_cnt)*100) ,"%" , "%.0f" % (active_2 * self.cellToArea), " ft^2")
        print("between ", self.machine_compaction_zone * 2, "and", (self.machine_compaction_zone *3), "    : Area",  "%.1f" % ((active_3/active_cnt)*100) ,"%", "%.0f" % (active_3 * self.cellToArea), " ft^2")
        print("between ", self.machine_compaction_zone * 3, "and", (self.machine_compaction_zone *4), "    : Area",  "%.1f" % ((active_4/active_cnt)*100) ,"%", "%.0f" % (active_4 * self.cellToArea), " ft^2")
        print("between ", self.machine_compaction_zone * 4, "and", (self.machine_compaction_zone *5), "    : Area",  "%.1f" % ((active_5/active_cnt)*100) ,"%", "%.0f" % (active_5 * self.cellToArea), " ft^2")
        print("        ",self.machine_compaction_zone * 5, "and above   : Area",  "%.1f" % ((active_6/active_cnt)*100) ,"%", "%.0f" % (active_6 * self.cellToArea), " ft^2")


        print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Thick-Lift summary >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
        print(">>>>>> Thick lift flag - count      :" , total_cnt_thick) # this is all thick lifts including repeats in single cell
        print(">>>>>> Thick lift      - coverage   :", "%.4f" % (total_cnt_thick * self.AreatoAcre), " acres") # this is thick lift coverage, not all cells

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
        
        END NOT CURRENTLY USED '''

    def create_excel_report(self, contributing_machines,
                            total_duration,
                            cell_pass_count_total,
                            ne_dict,
                            bin_pass_counts,
                            cell_summaries,
                            machine_operation_dict):

        compaction_evaulation = CompactionEvaluation(self.machine_compaction_zone)
        compaction_evaulation.evaluate_compaction(ne_dict, cell_summaries)




        ############################
        ##Determine what time events took place and what machines were involved for Excel outputs
        ############################

        #blah_index = 0
        # events per hour by machine
        machine_volume_eventDict = {"mitigated_volume": {}, }
        machine_volume_eventDict_positive = {"unmitigated_volume": {}, }
        machine_volume_eventDict_negative = {"remediation_volume": {}, }

        machine_area_eventDict = {"mitigated_area": {}, }
        machine_area_eventDict_positive = {"unmitigated_area": {}, }
        machine_area_eventDict_negative = {"remediation_area": {}, }

        machine_lift_Dict = {"lifts": {}, }

        for current_machine in contributing_machines:
            #blah = list(contributing_machines)
            #check_machine = blah[blah_index]
            #blah_index = blah_index + 1
            machine_volume_events_all = {}
            machine_volume_events_positive = {}
            machine_volume_events_negative = {}
            machine_area_events_all = {}
            machine_area_events_negative = {}
            machine_area_events_positive = {}
            lift_summary = {'0 to 0.5': 0, 'B': 0, 'C': 0, 'D': 0, 'E': 0, 'F': 0}
            eventMagnitude = 0.0

            # seperate events by machine, for each machine tally unmitigated volume, mitigated volume and remediation volume
            for cell, passes in ne_dict.items():
                for cell_pass in passes:
                    if cell_pass['machine'] == current_machine:
                        current_time = (cell_pass['dateTime'])
                        # this takes (+) and (-) to determine the mitigated volume
                        if current_time.hour not in machine_volume_events_all:
                            machine_area_events_all[current_time.hour] = 1
                            machine_volume_events_all[current_time.hour] = cell_pass['EventMagnitude']
                        else:
                            machine_volume_events_all[current_time.hour] = machine_volume_events_all[
                                                                               current_time.hour] + cell_pass[
                                                                               'EventMagnitude']

                        # this records all positive volume events for a total of all volume events
                        if cell_pass['EventMagnitude'] > 0.0:
                            if current_time.hour not in machine_volume_events_positive:
                                machine_volume_events_positive[current_time.hour] = cell_pass['EventMagnitude']
                                machine_area_events_positive[current_time.hour] = 1
                                machine_area_events_all[current_time.hour] = 1
                            else:
                                machine_volume_events_positive[current_time.hour] = \
                                machine_volume_events_positive[current_time.hour] + cell_pass['EventMagnitude']
                                machine_area_events_positive[current_time.hour] += 1
                                machine_area_events_all[current_time.hour] += 1

                        #TODO What about 0.0?
                        # this records all remediation volume events
                        if cell_pass['EventMagnitude'] < 0.0:
                            if current_time.hour not in machine_volume_events_negative:
                                machine_volume_events_negative[current_time.hour] = cell_pass['EventMagnitude']
                                machine_area_events_negative[current_time.hour] = 1
                                machine_area_events_all[current_time.hour] = 1
                            else:
                                machine_volume_events_negative[current_time.hour] = \
                                    machine_volume_events_negative[current_time.hour] + cell_pass['EventMagnitude']
                                machine_area_events_negative[current_time.hour] += 1
                                machine_area_events_all[current_time.hour] -= 1

                        # get data for lift graph
                        lift_magnitude = cell_pass['delta_from_last_pass']
                        if lift_magnitude > 0.0:

                            if self.filter_elevation_uncertainty < lift_magnitude <= self.machine_compaction_zone:
                                lift_summary['0 to 0.5'] += 1

                            elif lift_magnitude > (self.machine_compaction_zone) and lift_magnitude <= (
                                    self.machine_compaction_zone * 2):
                                lift_summary['B'] += 1

                            elif lift_magnitude > (self.machine_compaction_zone * 2) and lift_magnitude <= (
                                    self.machine_compaction_zone * 3):
                                lift_summary['C'] += 1

                            elif lift_magnitude > (self.machine_compaction_zone * 3) and lift_magnitude <= (
                                    self.machine_compaction_zone * 4):
                                lift_summary['D'] += 1

                            elif lift_magnitude > (self.machine_compaction_zone * 4) and lift_magnitude <= (
                                    self.machine_compaction_zone * 5):
                                lift_summary['E'] += 1

                            elif lift_magnitude >= (self.machine_compaction_zone * 5):
                                lift_summary['F'] += 1

            # this is machine breakout data
            # lift
            machine_lift_Dict.setdefault(current_machine, []).append(lift_summary)
            # print(machine_lift_Dict)
            # volume
            machine_volume_eventDict.setdefault(current_machine, []).append(machine_volume_events_all)  # mitigated
            machine_volume_eventDict_positive.setdefault(current_machine, []).append(machine_volume_events_positive)  # unmitigated
            machine_volume_eventDict_negative.setdefault(current_machine, []).append(machine_volume_events_negative)  # remediation
            # area
            machine_area_eventDict.setdefault(current_machine, []).append(machine_area_events_all)  # mitigated
            machine_area_eventDict_positive.setdefault(current_machine, []).append(machine_area_events_positive)  # unmitigated
            machine_area_eventDict_negative.setdefault(current_machine, []).append(machine_area_events_negative)  # remediation

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

        for cell, passes in ne_dict.items():
            for cell_pass in passes:
                current_time = (cell_pass['dateTime'])

                # All
                if current_time.hour not in hour_volume_events_all:
                    # hour_area_events_all[current_time.hour] = 1
                    hour_volume_events_all[current_time.hour] = cell_pass['EventMagnitude']
                else:
                    hour_volume_events_all[current_time.hour] = hour_volume_events_all[current_time.hour] + \
                                                                cell_pass['EventMagnitude']

                # Negative
                if current_time.hour not in hour_volume_events_negative:
                    # hour_area_events_negative[current_time.hour] = 1
                    hour_volume_events_negative[current_time.hour] = cell_pass['EventMagnitude']
                else:
                    if cell_pass['EventMagnitude'] <= 0.0:
                        hour_volume_events_negative[current_time.hour] = hour_volume_events_negative[
                                                                             current_time.hour] + cell_pass[
                                                                             'EventMagnitude']

                # Positive
                if current_time.hour not in hour_volume_events_positive:
                    # hour_area_events_positive[current_time.hour] = 1
                    hour_volume_events_positive[current_time.hour] = cell_pass['EventMagnitude']
                else:
                    if cell_pass['EventMagnitude'] >= 0.0:
                        hour_volume_events_positive[current_time.hour] = hour_volume_events_positive[
                                                                             current_time.hour] + cell_pass[
                                                                             'EventMagnitude']

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

        #TODO What are we up to here?
        lst = list()
        for cell, v in (sorted(bin_pass_counts.items())):
            # print (k , "Passes " ,v, " occurances ")
            lst.append((int(cell), v))
        lst.sort()

        for cell, v in lst[:1]:
            single_pass_cells = v
        # End passcount sorting

        #################################################
        # Create Over,Under and thicklift surface files #
        ##################################################
        # Over compaction surface file
        #TODO move this elsewhere one day snakepit (probably) will not use this
        if self.create_points:
            OverSurfaceOut = "./output/" + self.current_file + "_OverCompaction.pts"
            OverOut = open(OverSurfaceOut, 'w')
            O = csv.writer(OverOut, delimiter=',', dialect='excel', lineterminator='\n')

            # Under compction surface file
            UnderSurfaceOut = "./output/" + self.current_file + "_Under_compaction.pts"
            UnderOut = open(UnderSurfaceOut, 'w')
            U = csv.writer(UnderOut, delimiter=',', dialect='excel', lineterminator='\n')

            # Thicklift surface file
            ThickSurfaceOut = "./output/" + self.current_file + "_ThickLift.pts"
            ThickOut = open(ThickSurfaceOut, 'w')
            Th = csv.writer(ThickOut, delimiter=',', dialect='excel', lineterminator='\n')

            # Volume Event surface file
            VolumeEventSurfaceOut = "./output/" + self.current_file + "_VolumeEvent.pts"
            VolumeEventOut = open(VolumeEventSurfaceOut, 'w')
            VE = csv.writer(VolumeEventOut, delimiter=',', dialect='excel', lineterminator='\n')

            # Coverage surface file
            CoverageSurfaceOut = "./output/" + self.current_file + "_coverage.pts"
            CoverageOut = open(CoverageSurfaceOut, 'w')
            CO = csv.writer(CoverageOut, delimiter=',', dialect='excel', lineterminator='\n')

            # Active surface file
            ActiveSurfaceOut = "./output/" + self.current_file + "_ActiveArea.pts"
            ActiveOut = open(ActiveSurfaceOut, 'w')
            ACT = csv.writer(ActiveOut, delimiter=',', dialect='excel', lineterminator='\n')

        # tally totals from indiviual cells
        total_cnt_thick = 0
        total_cnt_under = 0
        total_cnt_over = 0
        total_volume = 0.0
        total_cut = 0.0
        total_fill = 0.0
        total_event_volume = 0.0


        #Check
        # for cell, passes in ne_dict.items():
        #     East = cell[1]
        #     North = cell[0]

        for cell, summary in cell_summaries.items:
            North, East = cell

            #if "passes_for_cell" in summary:  # determine if cell compaction is present to be evaluated
            if self.create_points:
                CO.writerow([East, North, 0.0])
            # Pull volume information for each cell
            current_cell_volume = summary['volume']
            if current_cell_volume >= self.filter_low_volume_cell and self.create_points:
                ACT.writerow([East, North, current_cell_volume])

            #TODO these casts should not be required
            current_cell_cut = float(summary['cut'])
            current_cell_fill = float(summary['fill'])
            current_event_volume = float(summary['volume_event'])

            # Populate VolumeEvent surface
            if current_event_volume > 0 and self.create_points:
                VE.writerow([East, North, current_event_volume])

            # total volume information
            total_volume = float(total_volume) + float(current_cell_volume)
            total_cut = float(total_cut) + float(current_cell_cut)
            total_fill = float(total_fill) + float(current_cell_fill)
            total_event_volume = float(total_event_volume) + float(current_event_volume)

            # pull cell stats
            current_cnt_over = summary['cnt_over_compaction']
            current_cnt_under = summary['cnt_under_compaction']
            current_cnt_thick = summary['cnt_self.thick_lift']

            # Populate over, under and thick surface files
            if current_cnt_over > 0 and self.create_points:
                O.writerow([East, North, current_cnt_over])
            if current_cnt_under > 0 and self.create_points:
                U.writerow([East, North, current_cnt_under])
            if current_cnt_thick > 0 and self.create_points:
                Th.writerow([East, North, current_cnt_thick])

            # total cell stats
            total_cnt_thick = total_cnt_thick + current_cnt_thick
            total_cnt_under = total_cnt_under + current_cnt_under
            total_cnt_over = total_cnt_over + current_cnt_over

        if self.create_points:
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

        area_coverage = (len(ne_dict) * self.AreatoAcre)
        single_pass_area = single_pass_cells * self.AreatoAcre
        area_cut_cell = compaction_evaulation.cnt_cut_cells * self.AreatoAcre
        area_low_volume_cell = compaction_evaulation.cnt_low_volume_cells * self.AreatoAcre
        active_compaction_area = (compaction_evaulation.cnt_unique_cell_compaction_area * self.AreatoAcre)


        # Output file
        #output = "./output/CompactionOutput.csv"
        #fout = open(output, 'a')
        #w = csv.writer(fout, delimiter=',', dialect='excel', lineterminator='\n')
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
        #TODO: This might not be needed
        # w.writerow([self.current_file,  # 1
        #             len(contributing_machines),  # 2
        #             total_duration,  # 3
        #             "%.4f" % (cell_pass_count_total * self.AreatoAcre),  # 4
        #             # "%.4f" % (area_coverage) ,   #4.5
        #             # "%.4f" %  (single_pass_area) ,                              #5
        #             # "%.4f" % (area_low_volume_cell) ,                           #6
        #             # "%.4f" % (area_cut_cell) ,                                  #7
        #             "%.4f" % (active_compaction_area),  # 8
        #             "%.0f" % F,  # 9
        #             # "%.0f" % -C,                                               #10
        #             # "%.0f" % V0,                                                #11
        #             # "%.0f" % VR,                                                #11.1
        #             "%.0f" % VM,  # 11.2
        #             "%.0f" % ((under_compacted_volume_percentage) * 100),  # 12
        #             # "%.0f" % (double_handle_volume),                              #13
        #             lift_passes,  # 14
        #             "%.1f" % ((bin_1 / lift_passes) * 100),
        #             "%.1f" % ((bin_2 / lift_passes) * 100),
        #             "%.1f" % ((bin_3 / lift_passes) * 100),
        #             "%.1f" % ((bin_4 / lift_passes) * 100),
        #             "%.1f" % ((bin_5 / lift_passes) * 100),
        #             "%.1f" % ((bin_6 / lift_passes) * 100),
        #             compaction_passes,  # 21
        #             # "%.1f" % ((neg_1/compaction_passes)*100) ,
        #             # "%.1f" % ((neg_2/compaction_passes)*100) ,
        #             # "%.1f" % ((neg_3/compaction_passes)*100) ,
        #             # "%.1f" % ((neg_3/compaction_passes)*100) ,
        #             # "%.1f" % ((neg_5/compaction_passes)*100) ,
        #             # "%.1f" % ((neg_6/compaction_passes)*100) ,
        #             "%.1f" % ((total_cnt_over / compaction_passes) * 100),  # 28
        #             "%.0f" % (active_volume_1 * self.cellVoltoCubic),
        #             "%.0f" % (active_volume_2 * self.cellVoltoCubic),
        #             "%.0f" % (active_volume_3 * self.cellVoltoCubic),
        #             "%.0f" % (active_volume_4 * self.cellVoltoCubic),
        #             "%.0f" % (active_volume_5 * self.cellVoltoCubic),
        #             "%.0f" % (active_volume_6 * self.cellVoltoCubic),  # 34
        #
        #             ])
        # fout.close()

        # print(machine_area_eventDict)

        # prepare Dictionary for Excel output of undercompacted material distribution

        # prepare site summary for Excel

        F = float(total_fill * self.cellVoltoCubic)
        V0 = float(total_event_volume * self.cellVoltoCubic)
        VR = compaction_evaulation.remediation_under_compaction_events * self.cellVoltoCubic
        VM = float((V0 - VR))
        under_compacted_volume_percentage = VM / F
        double_handle_volume = compaction_evaulation.double_handle_cubic * self.cellVoltoCubic
        C = float(total_cut * self.cellVoltoCubic)
        V = float((total_fill + total_cut) * self.cellVoltoCubic)


        site_summary_to_excel = (
            ['file processed', self.filename],
            ['Thicklift value', self.thick_lift_threshold],
            ['Low volume cell filter', self.filter_low_volume_cell],
            ['uncertainty filter', self.filter_elevation_uncertainty],
            ['Machine compaction zone', self.machine_compaction_zone],
            ['Contributing machines', len(contributing_machines)],
            ['total hours', total_duration],
            ['Machine Operation Coverage area Acres', (cell_pass_count_total * self.AreatoAcre)],
            ['Coverage - Area(VL uses) Acres', area_coverage],
            ['Removed for single pass Acres', single_pass_area],
            ['Removed for Cut Acres', area_cut_cell],
            ['Removed for low volume Acres', area_low_volume_cell],
            ['Active compaction - area Acres', active_compaction_area],
            ['Volume of Fill yd^3', F],
            ['Total Volume under compacted yd^3', V0],
            ['Remediated under compacted yd^3', -VR],
            ['Remaining undercomapcted yd^3', VM],
            ['% volume under compacted', under_compacted_volume_percentage * 100],
            ['volume of double handle', double_handle_volume],
            ['volume of cut', -C],
            ['under compacted event % 0 to 0.5', (((compaction_evaulation.active_volume_1 * self.cellVoltoCubic) / V0) * 100)],
            ['under compacted event % 0.5 to 1.0', (((compaction_evaulation.active_volume_2 * self.cellVoltoCubic) / V0) * 100)],
            ['under compacted event % 1.0 to 1.5', (((compaction_evaulation.active_volume_3 * self.cellVoltoCubic) / V0) * 100)],
            ['under compacted event % 1.5 to 2.0', (((compaction_evaulation.active_volume_4 * self.cellVoltoCubic) / V0) * 100)],
            ['under compacted event % 2.0 to 2.5', (((compaction_evaulation.active_volume_5 * self.cellVoltoCubic) / V0) * 100)],
            ['under compacted event % 2.5 >', (((compaction_evaulation.active_volume_6 * self.cellVoltoCubic) / V0) * 100)],
            ['under compacted event volume 0 to 0.5', (compaction_evaulation.active_volume_1 * self.cellVoltoCubic)],
            ['under compacted event volume 0.5 to 1.0', (compaction_evaulation.active_volume_2 * self.cellVoltoCubic)],
            ['under compacted event volume 1.0 to 1.5', (compaction_evaulation.active_volume_3 * self.cellVoltoCubic)],
            ['under compacted event volume 1.5 to 2.0', (compaction_evaulation.active_volume_4 * self.cellVoltoCubic)],
            ['under compacted event volume 2.0 to 2.5', (compaction_evaulation.active_volume_5 * self.cellVoltoCubic)],
            ['under compacted event volume 2.5 >', (compaction_evaulation.active_volume_6 * self.cellVoltoCubic)],
            ['lift passes', compaction_evaulation.lift_passes],
            ['compaction passes', compaction_evaulation.compaction_passes],
            ['lift between filter and 0.5', ((compaction_evaulation.bin_1 / compaction_evaulation.lift_passes) * 100)],
            ['lift between 0.5 and 1.0', ((compaction_evaulation.bin_2 / compaction_evaulation.lift_passes) * 100)],
            ['lift between 1.0 and 1.5', ((compaction_evaulation.bin_3 / compaction_evaulation.lift_passes) * 100)],
            ['lift between 1.5 and 2.0', ((compaction_evaulation.bin_4 / compaction_evaulation.lift_passes) * 100)],
            ['lift between 2.0 and 2.5', ((compaction_evaulation.bin_5 / compaction_evaulation.lift_passes) * 100)],
            ['lift greater than 2.5', ((compaction_evaulation.bin_6 / compaction_evaulation.lift_passes) * 100)],

        )

        # create dictionary to display lift sumary by %
        lift_summaryDict = {"Lift Summary breakdown": {}, }
        lift_summary_breakdown_all = {}
        ucv = ((compaction_evaulation.bin_1 / compaction_evaulation.lift_passes) * 100)
        lift_summary_breakdown_all['0 to 0.5'] = ucv
        ucv = ((compaction_evaulation.bin_2 / compaction_evaulation.lift_passes) * 100)
        lift_summary_breakdown_all['0.5 to 1.0'] = ucv
        ucv = ((compaction_evaulation.bin_3 / compaction_evaulation.lift_passes) * 100)
        lift_summary_breakdown_all['1.0 to 1.5'] = ucv
        ucv = ((compaction_evaulation.bin_4 / compaction_evaulation.lift_passes) * 100)
        lift_summary_breakdown_all['1.5 to 2.0'] = ucv
        ucv = ((compaction_evaulation.bin_5 / compaction_evaulation.lift_passes) * 100)
        lift_summary_breakdown_all['2.0 to 2.5'] = ucv
        ucv = ((compaction_evaulation.bin_6 / compaction_evaulation.lift_passes) * 100)
        lift_summary_breakdown_all['> 2.5'] = ucv

        lift_summaryDict['Lift Summary breakdown'] = [lift_summary_breakdown_all]

        # create dictionary to display undercompacted material magnitude breakdown by %
        under_compacted_material_magnitude_breakdownDict = {"Under compacted breakdown": {}, }
        under_compacted_breakdown_all = {}
        ucv = (compaction_evaulation.active_volume_1 * self.cellVoltoCubic)
        under_compacted_breakdown_all['0 to 0.5'] = ucv
        ucv = (compaction_evaulation.active_volume_2 * self.cellVoltoCubic)
        under_compacted_breakdown_all['0.5 to 1.0'] = ucv
        ucv = (compaction_evaulation.active_volume_3 * self.cellVoltoCubic)
        under_compacted_breakdown_all['1.0 to 1.5'] = ucv
        ucv = (compaction_evaulation.active_volume_4 * self.cellVoltoCubic)
        under_compacted_breakdown_all['1.5 to 2.0'] = ucv
        ucv = (compaction_evaulation.active_volume_5 * self.cellVoltoCubic)
        under_compacted_breakdown_all['2.0 to 2.5'] = ucv
        ucv = (compaction_evaulation.active_volume_6 * self.cellVoltoCubic)
        under_compacted_breakdown_all['> 2.5'] = ucv

        under_compacted_material_magnitude_breakdownDict['Under compacted breakdown'] = [under_compacted_breakdown_all]

        # create dictionary to display undercompacted material volume by magnitude
        under_compacted_volume_breakdownDict = {"Under compacted vol breakdown": {}, }
        under_compacted_volume_breakdown_all = {}
        ucv = int((((compaction_evaulation.active_volume_1 * self.cellVoltoCubic) / VM) * 100))
        under_compacted_volume_breakdown_all['0 to 0.5'] = ucv
        ucv = int((((compaction_evaulation.active_volume_2 * self.cellVoltoCubic) / VM) * 100))
        under_compacted_volume_breakdown_all['0.5 to 1.0'] = ucv
        ucv = int((((compaction_evaulation.active_volume_3 * self.cellVoltoCubic) / VM) * 100))
        under_compacted_volume_breakdown_all['1.0 to 1.5'] = ucv
        ucv = int((((compaction_evaulation.active_volume_4 * self.cellVoltoCubic) / VM) * 100))
        under_compacted_volume_breakdown_all['1.5 to 2.0'] = ucv
        ucv = int((((compaction_evaulation.active_volume_5 * self.cellVoltoCubic) / VM) * 100))
        under_compacted_volume_breakdown_all['2.0 to 2.5'] = ucv
        ucv = int((((compaction_evaulation.active_volume_6 * self.cellVoltoCubic) / VM) * 100))
        under_compacted_volume_breakdown_all['> 2.5'] = ucv

        under_compacted_volume_breakdownDict['Under compacted vol breakdown'] = [under_compacted_volume_breakdown_all]



        ##########################################################
        # create Volume dictionary and write to .xlxs file
        ##########################################################
        ExcelOutputFile = "./output/" + self.current_file + ".xlsx"

        tempVolumeDict = {}  # mitigated volume
        tempVolumeDict_accumulated = {}

        for machine, passes in machine_volume_eventDict.items():
            accumulated_vol = 0.0
            for times in passes:
                lst = list(times.keys())
                lst.sort()
                for key in lst:
                    vol = float(times[key] * self.cellVoltoCubic)
                    accumulated_vol = accumulated_vol + vol
                    vol = round(vol, 0)
                    accumulated_vol = round(accumulated_vol, 0)
                    tempVolumeDict.setdefault(machine, {})[key] = vol
                    tempVolumeDict_accumulated.setdefault(machine, {})[key] = accumulated_vol

        df1 = pd.DataFrame(tempVolumeDict)
        df100 = pd.DataFrame(tempVolumeDict_accumulated)
        # print(tempVolumeDict_accumulated)

        tempVolumeDict = {}  # undercompacted material breakdown
        # print(under_compacted_material_magnitude_breakdownDict)
        for machine, passes in under_compacted_material_magnitude_breakdownDict.items():

            for times in passes:
                lst = list(times.keys())
                lst.sort()
                for key in lst:
                    vol = int(times[key])
                    # print(vol)
                    # vol = round(vol,0)
                    tempVolumeDict.setdefault(machine, {})[key] = vol
        df5 = pd.DataFrame(tempVolumeDict)

        tempVolumeDict = {}  # undercompacted material breakdown
        # print(under_compacted_material_magnitude_breakdownDict)
        for machine, passes in under_compacted_volume_breakdownDict.items():

            for times in passes:
                lst = list(times.keys())
                lst.sort()
                for key in lst:
                    vol = int(times[key])
                    # print(vol)
                    # vol = round(vol,0)
                    tempVolumeDict.setdefault(machine, {})[key] = vol
        df6 = pd.DataFrame(tempVolumeDict)

        # Lift Summary breakdown
        tempVolumeDict = {}  # Lift Summary breakdown
        # print(under_compacted_material_magnitude_breakdownDict)
        for machine, passes in lift_summaryDict.items():
            for times in passes:
                lst = list(times.keys())
                lst.sort()
                for key in lst:
                    vol = int(times[key])
                    # print(vol)
                    # vol = round(vol,0)
                    tempVolumeDict.setdefault(machine, {})[key] = vol
        df7 = pd.DataFrame(tempVolumeDict)

        tempVolumeDict = {}  # unmitigated volume
        for machine, passes in machine_volume_eventDict_positive.items():

            for times in passes:
                lst = list(times.keys())
                lst.sort()
                for key in lst:
                    vol = float(times[key] * self.cellVoltoCubic)
                    # vol = round(vol,0)
                    tempVolumeDict.setdefault(machine, {})[key] = vol
        df2 = pd.DataFrame(tempVolumeDict)

        tempVolumeDict = {}  # remediated volume
        tempVolumeDict_accumulated = {}
        for machine, passes in machine_volume_eventDict_negative.items():
            accumulated_remediation = 0.0
            for times in passes:
                lst = list(times.keys())
                lst.sort()
                for key in lst:
                    vol = float(times[key] * self.cellVoltoCubic)
                    vol = round(vol, 0)
                    accumulated_remediation = accumulated_remediation + vol
                    accumulated_remediation = round(accumulated_remediation)
                    tempVolumeDict.setdefault(machine, {})[key] = vol
                    tempVolumeDict_accumulated.setdefault(machine, {})[key] = accumulated_remediation
        df3 = pd.DataFrame(tempVolumeDict)
        df300 = pd.DataFrame(tempVolumeDict_accumulated)

        tempAreaDict = {}
        for machine, passes in machine_area_eventDict.items():
            for times in passes:
                lst = list(times.keys())
                # print("max row",max_row)
                lst.sort()
                for key in lst:
                    sqft = float(times[key] * self.cell_to_area)
                    sqft = round(sqft, 0)
                    tempAreaDict.setdefault(machine, {})[key] = sqft

        df10 = pd.DataFrame(tempAreaDict)

        # create sheets
        writer = pd.ExcelWriter(ExcelOutputFile, engine='xlsxwriter')
        workbook = writer.book
        worksheetSummary = workbook.add_worksheet('Site summary')
        df1.to_excel(writer, 'Mitigated_Volume')
        df100.to_excel(writer, 'Mitigated Volume accum')
        # df2.to_excel(writer,'(+) vol evnt')
        df3.to_excel(writer, '(-) vol evnt')
        df300.to_excel(writer, '(-) vol evnt accum')
        df5.to_excel(writer, 'undercompacted volume breakdown')
        # df6.to_excel(writer,'undercompacted event breakdown')
        df7.to_excel(writer, 'lift summary breakdown')
        # df10.to_excel(writer,'Area Events')

        ###############################################
        # Site summary information
        row = 0
        col = 0
        for item, value in site_summary_to_excel:
            worksheetSummary.write(row, col, item)
            worksheetSummary.write(row, col + 1, value)
            row += 1

        max_row = 13
        ###############################################
        # df100 create chart for mitigated volume acumulation
        worksheet100 = writer.sheets['Mitigated Volume accum']
        chart = workbook.add_chart({'type': 'line'})
        for i in range(len(machine_volume_eventDict.items())):
            col = i + 1
            chart.add_series({
                'name': ['Mitigated Volume accum', 0, col],
                'categories': ['Mitigated Volume accum', 1, 0, max_row, 0],
                'values': ['Mitigated Volume accum', 1, col, max_row, col],
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
                'name': ['Mitigated_Volume', 0, col],
                'categories': ['Mitigated_Volume', 1, 0, max_row, 0],
                'values': ['Mitigated_Volume', 1, col, max_row, col],
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
                'name': ['(-) vol evnt', 0, col],
                'categories': ['(-) vol evnt', 1, 0, max_row, 0],
                'values': ['(-) vol evnt', 1, col, max_row, col],
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
                'name': ['(-) vol evnt accum', 0, col],
                'categories': ['(-) vol evnt accum', 1, 0, max_row, 0],
                'values': ['(-) vol evnt accum', 1, col, max_row, col],
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
                'name': ['undercompacted volume breakdown', 0, col],
                'categories': ['undercompacted volume breakdown', 1, 0, max_row, 0],
                'values': ['undercompacted volume breakdown', 1, col, max_row, col],
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
                'name': ['lift summary breakdown', 0, col],
                'categories': ['lift summary breakdown', 1, 0, max_row, 0],
                'values': ['lift summary breakdown', 1, col, max_row, col],
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
