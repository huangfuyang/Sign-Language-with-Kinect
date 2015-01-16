import sys
from os import listdir
from os.path import join,dirname,realpath,isdir
import json
from csv_reader import CSVReader
from body_box_extractor import BodyBoxExtractor

ROOT_DIRECTORY = join(dirname(realpath(sys.argv[0])), '..', '..', 'res')
data_directories = [f for f in listdir(ROOT_DIRECTORY) if isdir(join(ROOT_DIRECTORY, f))]
csv_reader = CSVReader(ROOT_DIRECTORY)
body_box_extractor = BodyBoxExtractor('', None)
check_results = {}

for data_directory in data_directories:
    try:
        type_json_file = open(join(ROOT_DIRECTORY, data_directory, 'type.json'))
        type_data = json.load(type_json_file)

        csv_reader.set_file(join(data_directory, data_directory+'.csv'))
        skeleton_data = {'raw_data': csv_reader.read()}
        data = body_box_extractor.process(skeleton_data)

        check_results[data_directory] = type_data
        check_results[data_directory]['has_shoulder'] = all(abs(data['body_box']['shoulder_left']['3d']['mean']) > 0.001) and all(abs(data['body_box']['shoulder_left']['3d']['mean']) > 0.001)
        check_results[data_directory]['has_hip'] = all(abs(data['body_box']['hip_left']['3d']['mean']) > 0.001) and all(abs(data['body_box']['hip_right']['3d']['mean']) > 0.001)

    except IOError:
        continue

selected_signs = [{data_name: data} for data_name,data in check_results.items() if data['has_hip'] and data['oneOrTwo'] is 1 and data['handRecognized'] and data['difficulty'] <= 2]
print sorted(selected_signs)
