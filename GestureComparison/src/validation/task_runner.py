class TaskRunner(object):

    preprocessed_data = []

    def __init__(self, reader, preprocessors, processor):
        self.reader = reader
        self.preprocessors = preprocessors
        self.processor = processor
        self.remove_data()

    def remove_data(self):
        self.preprocessed_data = []

    def add_data(self):
        try:
            data = self.reader.read()
        except AttributeError:
            print 'Error: No reader is defined'
            return

        try:
            filtered_data = data
            for preprocessor in self.preprocessors:
                filtered_data = preprocessor.preprocess(filtered_data)

            self.preprocessed_data.append(filtered_data)
        except AttributeError:
            pass


    def process(self):
        try:
            self.processor.process(self.preprocessed_data)
        except AttributeError:
            print 'Error: No processor is defined'
            return
