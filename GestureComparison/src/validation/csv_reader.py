from os.path import join
import numpy as np

class CSVReader(object):

    def __init__(self, root_directory):
        self.root_directory = root_directory

    def set_file(self, filename):
        self.filename = filename

    def read(self):
        # Read file
        src_file = open(join(self.root_directory, self.filename), 'rU')
        read_lines = src_file.readlines()
        src_file.close()

        arr = None
        no_of_dropped_frame = 0
        for i in xrange(0,len(read_lines)):
            current_line = read_lines[i]

            if current_line.startswith('untracked'):
                # Ignore untracked frames and reset
                no_of_dropped_frame = 0
            elif current_line.startswith('null'):
                # Count number of dropped frames for interpolation
                no_of_dropped_frame = no_of_dropped_frame + 1
            else:
                current_line_data = np.fromstring(current_line, sep=',')

                if arr is None:
                    arr = np.atleast_2d(current_line_data)
                else:
                    if no_of_dropped_frame > 0:
                        # Check for dropped frame
                        last_data = arr[-1,:]
                        diff_data = (current_line_data-last_data) / (no_of_dropped_frame+1)

                        # Fill dropped frames using linear interpolation
                        for i in xrange(1,no_of_dropped_frame+1):
                            temp_data = last_data + diff_data * i
                            arr = np.vstack((arr, temp_data))

                        no_of_dropped_frame = 0

                    arr = np.vstack((arr, current_line_data))

        return arr
