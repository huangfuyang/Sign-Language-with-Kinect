import sys
from os.path import join,dirname,realpath

from bspline_presenter import BSplinePresenter
from dtw_presenter import DTWPresenter

from csv_reader import CSVReader
from hand_selector import HandSelector
from bspline_preprocessor import BSplinePreprocessor
from dtw_processor import DTWProcessor
from task_runner import TaskRunner

ROOT_DIRECTORY = join(dirname(realpath(sys.argv[0])), '..', '..')

left_hand = False
right_hand = True
smoothing = 0.05
sampling_rate = 100

bspline_presenter = BSplinePresenter("B-Spline Presenter")
dtw_presenter = DTWPresenter("DTW Presenter")

csv_reader = CSVReader(ROOT_DIRECTORY)
hand_selector = HandSelector("Hand Selector", left_hand, right_hand)
bspline_preprocessor = BSplinePreprocessor("B-Spline Preprocessor", smoothing, sampling_rate, [bspline_presenter])
dtw_processor = DTWProcessor("DTW Processor", 2, [dtw_presenter])
task_runner = TaskRunner(csv_reader, [hand_selector, bspline_preprocessor], dtw_processor)

input_files = ['res/HKG_001_a_0001 Aaron 11/HKG_001_a_0001 Aaron 11.csv', 'res/HKG_001_a_0001 Aaron 22/HKG_001_a_0001 Aaron 22.csv']
for input_file in input_files:
    csv_reader.set_file(input_file)
    task_runner.add_data()
task_runner.process()
