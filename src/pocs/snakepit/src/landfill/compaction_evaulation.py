class CompactionEvaluation:

    def __init__(self, machine_compaction_zone, low_volume_cell_threshold, filter_elevation_uncertainty, double_handle_threshold, thick_lift_threshold ):
        self.cnt_unique_cell_compaction_area = 0
        self.bin_0 = self.bin_1 = self.bin_2 = self.bin_3 = self.bin_4 = self.bin_5 = self.bin_6 = 0
        self.neg_1 = self.neg_2 = self.neg_3 = self.neg_4 = self.neg_5 = self.neg_6 = 0
        self.active_0 = self.active_1 = self.active_2 = self.active_3 = self.active_4 = self.active_5 = self.active_6 = 0
        self.active_volume_1 = self.active_volume_2 = self.active_volume_3 = self.active_volume_4 = self.active_volume_5 = self.active_volume_6 = 0
        # active_volume_list = []
        self.positive_passes = 0
        self.negative_passes = 0
        # lift_passes = 0
        # compaction_passes = 0
        # positive_passes_greater_than_filter = 0
        self.active_cnt = 0
        self.cnt_low_volume_cells = 0
        self.cnt_cut_cells = 0
        self.cnt_cut_passes = 0
        self.cnt_low_volume_passes = 0
        self.cnt_lift_uncertainty = 0
        self.cnt_compact_uncertainty = 0
        self.double_handle_cubic = 0.0
        self.remediation_under_compaction_events = 0.0
        self.machine_compaction_zone = machine_compaction_zone
        self.low_volume_cell_threshold = low_volume_cell_threshold
        self.filter_elevation_uncertainty = filter_elevation_uncertainty
        self.double_handle_threshold = double_handle_threshold
        self.thick_lift_threshold = thick_lift_threshold
        self.lift_passes = 0
        self.compaction_passes = 0

    # TODO lift this into compcation evaulation
    def evaluate_compaction(self, ne_dict: dict, cell_summaries: dict):
        #######################################
        # Start of determining Machines and operating time #
        #######################################

        contributing_machines = self.get_contributing_machines(ne_dict)
        machine_duration, machine_operation_dict = self.analyse_machine_operation(contributing_machines,
                                                                                  ne_dict)

        ###################
        # summarize and evaluate individual cells #
        ###################
        # cnt_unique_cell_compaction_area = 0
        # bin_0 = bin_1 = bin_2 = bin_3 = bin_4 = bin_5 = bin_6 = 0
        # neg_1 = neg_2 = neg_3 = neg_4 = neg_5 = neg_6 = 0
        # active_0 = active_1 = active_2 = active_3 = active_4 = active_5 = active_6 = 0
        # active_volume_1 = active_volume_2 = active_volume_3 = active_volume_4 = active_volume_5 = active_volume_6 = 0
        # # active_volume_list = []
        # positive_passes = 0
        # negative_passes = 0
        # # lift_passes = 0
        # # compaction_passes = 0
        # # positive_passes_greater_than_filter = 0
        # active_cnt = 0
        # cnt_low_volume_cells = 0
        # cnt_cut_cells = 0
        # cnt_cut_passes = 0
        # cnt_low_volume_passes = 0
        # cnt_lift_uncertainty = 0
        # cnt_compact_uncertainty = 0
        # double_handle_cubic = 0.0
        # remediation_under_compaction_events = 0.0

        for cell, passes in ne_dict.items():
            # PassElevDict = {}
            PassElevList = []
            if cell in cell_summaries:
                # for vol in passes:  # determine if cell compaction should be evaluated for specific cell based on volume of cell
                current_elevation = None
                event_elevation_bottom_active_material = {}
                volume = cell_summaries['volume']
                passes_for_cell = cell_summaries['passes_for_cell']

                if volume < 0:
                    self.cnt_cut_cells += 1
                    self.cnt_cut_passes += passes_for_cell

                if  0 <= volume <= self.low_volume_cell_threshold:
                    self.cnt_low_volume_cells += 1
                    self.cnt_low_volume_passes += passes_for_cell
                # this is in the wrong place?

                # TODO Why do this?
                # filter out cells with little volume from surface area but not volume calcs
                if volume > self.low_volume_cell_threshold:
                    # these are the remaining unique cells which were not filtered out
                    self.cnt_unique_cell_compaction_area += 1
                    compaction_state = 1  # TODO meaning of this?
                    active_material = 0.0
                    event_elevation_counter = 0
                    event_elevation_bottom = []
                    # create a pass and elevation dictionary for current cell  !!!!Is this ordered correctly? #TODO what order should it be in?
                    for cell_pass in passes:  # pull out pass and elevation data and put in a pass and elevation dictionary
                        if current_elevation is None:
                            previous_elevation = cell_pass['elevation']
                        current_elevation = cell_pass['elevation']
                        delta_elevation = current_elevation - previous_elevation
                        previous_elevation = current_elevation  # this is used to get the bottom of an elevation event
                        cell_pass['delta_from_last_pass'] = delta_elevation
                        layer = delta_elevation
                        PassElevList.append(cell_pass['elevation'])

                        ####################################
                        # Single cell Evaluate compaction  #
                        ####################################

                        # for layer in elev_delta_list:
                        # This area evalutes layers and records under compaction and Thick lift events for a single cell
                        # - Full compaction state = 1.  Under compaction states are 0
                        # - Under compaction event is defined as placing material on top of under compacted material.
                        # - Compactiuon state goes to 1  when pass to pass compaction is between zero and the (-) MCZ value

                        ##################
                        # Movement Down  #
                        ##################
                        # Compaction state is set to complete with movement down less than MCZ value
                        if layer <= 0.0:  # evaluate compaction layers
                            if layer >= -self.machine_compaction_zone:  # this is the on only condition that sets compaction state to 1
                                if compaction_state == 1:  # Fully compacted material was compacted more
                                    if layer < -self.filter_elevation_uncertainty:
                                        cell_summaries[cell][
                                            'cnt_over_compaction'] += 1  # keep track of overcompaction effort
                                compaction_state = 1  # set as compacted
                                active_material = 0  # compaction is complete, no active material
                                # print("reset compaction zone down")

                            else:  # moved down more than MCZ, this is normal compaction
                                compaction_state = 0  # every time compaction is more than MCZ, full compaction is required
                                active_material += layer  # Active material reduces because layer is negative

                                # CHECK if movement down mitigated an under compaction event
                                if active_material < 0.0:  # movement down is larger than the lift, material may have been removed
                                    # Compare the current elevation to the list of "bottom" of events.  If the current elevation is lower than a bottom,
                                    # find the active material associated with that event and add it to the mitigation tally.
                                    # Also pop the elevation of the event of the list and remove it from teh dictionaty
                                    if len(event_elevation_bottom) > 0:
                                        reverse_elevent_elevation_bottom = list(
                                            reversed(event_elevation_bottom))  # TODO this variable is not used

                                    for bottom in event_elevation_bottom:
                                        if current_elevation < bottom:  ## I don't think this works ??? #TODO DO we need to do something about that?
                                            # print(current_elevation, " current elevation < bottom", bottom)
                                            # if bottom in event_elevation_bottom:
                                            # thing = event_elevation_bottom[str(bottom)]
                                            # print("found it", thing)

                                            remediation = event_elevation_bottom_active_material[bottom]
                                            cell_pass[
                                                'EventMagnitude'] = -remediation  # Assign magnitude of event to pass of machine
                                            # if remediation > 1.0:
                                            # print(remediation)
                                            # print(event_elevation_bottom)
                                            popped = event_elevation_bottom.pop()  # TODO not used
                                            # print(" current elev, popped", current_elevation,popped)
                                            self.remediation_under_compaction_events += remediation
                                            # remove remediated event from event magnitude distribution
                                            # figure out which machine removed the event???
                                            self.active_cnt -= 1
                                            if remediation <= self.machine_compaction_zone:
                                                self.active_1 -= 1
                                                self.active_volume_1 -= remediation
                                            elif self.machine_compaction_zone < remediation <= self.machine_compaction_zone * 2:
                                                self.active_2 -= 1
                                                self.active_volume_2 -= remediation
                                            elif self.machine_compaction_zone * 2 < remediation <= self.machine_compaction_zone * 3:
                                                self.active_3 -= 1
                                                self.active_volume_3 -= remediation
                                            elif self.machine_compaction_zone * 3 < remediation <= self.machine_compaction_zone * 4:
                                                self.active_4 -= 1
                                                self.active_volume_4 -= remediation
                                            elif self.machine_compaction_zone * 4 < remediation <= self.machine_compaction_zone * 5:
                                                self.active_5 -= 1
                                                self.active_volume_5 -= remediation
                                            else:
                                                self.active_6 -= 1
                                                self.active_volume_6 -= remediation
                                    active_material = 0.0
                                    # print("____________________________")
                                    # print("reset active material < 0")

                        ################
                        # Movement UP  #
                        ################
                        # This is where event volumes occur, if material is placed on top of uncompacted material, the active material is the event volume
                        if layer > 0.0:
                            if layer <= self.machine_compaction_zone:  # Material added within MCZ
                                if compaction_state == 1:  # compaction was complete, no under compaction event
                                    active_material = 0
                            elif self.machine_compaction_zone < layer < self.thick_lift_threshold:  # Material added more than MCZ but less than TL
                                if compaction_state == 1:  # compaction was complete, no under compaction event
                                    active_material = layer
                                    # print(active_material)
                                    compaction_state = 0  # movement was up greater than MCZ so reset compaction state
                                else:  # UNDER COMPACTION EVENT >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                                    cell_summaries[cell][
                                        'cnt_under_compaction'] += 1  # put under compaction event information into summary dictionary
                                    cell_summaries[cell][
                                        'volume_event'] += active_material  # add up total volume of all evants for cell
                                    cell_pass[
                                        'EventMagnitude'] = active_material  # Assign magnitude of event to pass of machine
                                    event_adjusted_for_active = previous_elevation - active_material  # this gets us to the bottom of the previous lift
                                    event_elevation_bottom.append(event_adjusted_for_active)
                                    event_elevation_counter += 1
                                    # add event bottom and active material to a dictionary so that if mitigated, the active material contribution to under
                                    # compacted volume can be added to the undercompacted event mitigation tally
                                    event_elevation_bottom_active_material[
                                        event_adjusted_for_active] = active_material
                                    # print (event_elevation_bottom_active_material)

                                    #  END of UNDER COMPATION EVENT  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                                    self.active_cnt += 1  # ???
                                    if active_material <= self.machine_compaction_zone:
                                        self.active_1 += 1
                                        self.active_volume_1 += active_material
                                    elif self.machine_compaction_zone < active_material <= self.machine_compaction_zone * 2:
                                        self.active_2 += 1
                                        self.active_volume_2 += active_material
                                    elif self.machine_compaction_zone * 2 < active_material <= self.machine_compaction_zone * 3:
                                        self.active_3 += 1
                                        self.active_volume_3 += active_material
                                    elif self.machine_compaction_zone * 3 < active_material <= self.machine_compaction_zone * 4:
                                        self.active_4 += self.active_4 + 1
                                        self.active_volume_4 += active_material
                                    elif self.machine_compaction_zone * 4 < active_material <= self.machine_compaction_zone * 5:
                                        self.active_5 += 1
                                        self.active_volume_5 += active_material
                                    else:
                                        self.active_6 += 1
                                        self.active_volume_6 += active_material

                                    active_material = layer  # + active_material ???  Only layer becasue active material has been recorded as under compacted event

                            else:  # A thick lift occured
                                if compaction_state == 1:  # compaction was complete, no under compaction event
                                    active_material = layer
                                    compaction_state = 0  # movement was up greater than MCZ so reset compaction state
                                else:  # UNDER COMPACTION EVENT >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                                    cell_summaries[cell][
                                        'cnt_under_compaction'] += 1  # put under compaction event information into summary dictionary
                                    cell_summaries[cell][
                                        'volume_event'] += active_material  # put under compaction event information into summary dictionary
                                    cell_pass[
                                        'EventMagnitude'] = active_material  # Assign magnitude of event to pass of machine
                                    event_adjusted_for_active = previous_elevation - active_material  # this gets us to the bottom of the previous lift
                                    event_elevation_bottom.append(event_adjusted_for_active)
                                    event_elevation_counter = event_elevation_counter + 1
                                    # add event bottom and active material to a dictionary so that if mitigated, the active material contribution to under
                                    # compacted volume can be added to the undercompacted event mitigation tally
                                    event_elevation_bottom_active_material[
                                        event_adjusted_for_active] = active_material
                                    # print ("thick",event_elevation_bottom_active_material)
                                    #  END of UNDER COMPATION EVENT  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                                    # print("before",active_material)
                                    self.active_cnt += 1
                                    if active_material <= self.machine_compaction_zone:
                                        self.active_1 += 1
                                        self.active_volume_1 += active_material
                                    elif self.machine_compaction_zone < active_material <= self.machine_compaction_zone * 2:
                                        self.active_2 += 1
                                        self.active_volume_2 += active_material
                                    elif self.machine_compaction_zone * 2 < active_material <= self.machine_compaction_zone * 3:
                                        self.active_3 += 1
                                        self.active_volume_3 += active_material
                                    elif self.machine_compaction_zone * 3 < active_material <= self.machine_compaction_zone * 4:
                                        self.active_4 += 1
                                        self.active_volume_4 += active_material
                                    elif self.machine_compaction_zone * 4 < active_material <= self.machine_compaction_zone * 5:
                                        self.active_5 += 1
                                        self.active_volume_5 += active_material
                                    else:
                                        self.active_6 += 1
                                        self.active_volume_6 += active_material

                                    active_material = layer  # + active_material ???  Only layer becasue active material has been recorded as under compacted event
                                    # print(active_material)

                                cell_summaries[cell][
                                    'cnt_self.thick_lift'] += 1  # put thick lift information into summary dictionary
                        # end of up movement evaluation
                        ################
                        # #Layer summary
                        ################
                        if layer <= 0.0:  # evaluate compaction layers
                            self.negative_passes += 1
                            if layer > -(
                                    self.machine_compaction_zone) and layer < -self.filter_elevation_uncertainty:
                                self.neg_1 += 1
                            elif layer < -(self.machine_compaction_zone) and layer >= -(
                                    self.machine_compaction_zone * 2):
                                self.neg_2 += 1
                            elif layer < -(self.machine_compaction_zone * 2) and layer >= -(
                                    self.machine_compaction_zone * 3):
                                self.neg_3 += 1
                            elif layer < -(self.machine_compaction_zone * 3) and layer >= -(
                                    self.machine_compaction_zone * 4):
                                self.neg_4 += 1
                            elif layer < -(self.machine_compaction_zone * 4) and layer >= -(
                                    self.machine_compaction_zone * 5):
                                self.neg_5 += 1
                            elif layer <= -(self.machine_compaction_zone * 5):
                                self.neg_6 += 1
                            # one more else to catch the rest....
                            if (layer > -(self.filter_elevation_uncertainty)):
                                self.cnt_compact_uncertainty += 1
                            if (layer < self.double_handle_threshold):
                                self.double_handle_cubic += layer

                        if layer > 0.0:
                            self.positive_passes += 1
                            if layer <= self.filter_elevation_uncertainty:
                                self.cnt_lift_uncertainty += 1
                                self.bin_0 +=  1
                            elif self.filter_elevation_uncertainty < layer <= self.machine_compaction_zone:
                                self.bin_1 += 1
                            elif layer > (self.machine_compaction_zone) and layer <= (
                                    self.machine_compaction_zone * 2):
                                self.bin_2 += 1
                            elif layer > (self.machine_compaction_zone * 2) and layer <= (
                                    self.machine_compaction_zone * 3):
                                self.bin_3 += 1
                            elif layer > (self.machine_compaction_zone * 3) and layer <= (
                                    self.machine_compaction_zone * 4):
                                self.bin_4 += 1
                            elif layer > (self.machine_compaction_zone * 4) and layer <= (
                                    self.machine_compaction_zone * 5):
                                self.bin_5 += 1
                            elif layer >= (self.machine_compaction_zone * 5):
                                self.bin_6 += 1
        ############################
        # End Evaluate compaction  #
        ############################
        self.__update_stats_after_evaulation__()

    def __update_stats_after_evaulation__(self):
        self.lift_passes = self.positive_passes - self.cnt_lift_uncertainty
        self.compaction_passes = self.negative_passes - self.cnt_compact_uncertainty



