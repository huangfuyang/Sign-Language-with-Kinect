class AbstractTask(object):
    def __init__(self, name):
        self.name = name

    def timing(self, run_function, data):
        import time

        start = time.clock()
        return_val = run_function(data)
        end = time.clock()

        print '"%s" used %fs'%(self.name, end-start)

        return return_val

class AbstractProcessor(AbstractTask):
    def __init__(self, name, presenters=None):
        super(AbstractProcessor, self).__init__(name)
        self.presenters = presenters

    def doProcess(self, data):
        processed_data = self.timing(self.process, data)

        if(hasattr(self, 'presenters') and self.presenters is not None):
            for i in xrange(0, len(self.presenters)):
                self.presenters[i].timing(self.presenters[i].display, processed_data)

        return processed_data


    def process(self, data):
        raise NotImplementedError("Subclasses of AbstractProcessor should implement this!")

class AbstractPresenter(AbstractTask):

    def __init__(self, name):
        super(AbstractPresenter, self).__init__(name)

    def display(self, data):
        raise NotImplementedError("Subclasses of AbstractPesenter should implement this!")
