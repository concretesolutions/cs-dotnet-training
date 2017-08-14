module.exports = {

  create: () => {

    let self = {
      _timeStart: null,
      _dateStart: null,
      _wasStarted: false,
      _dateStop: null,
      _elapsedTime: null,
      _elapsedMiliSeconds: null,
      _wasStoped: false
    };

    self.getTimeStart = () => {
      return self._timeStart;
    };

    self.getDateStart = () => {
      return self._dateStart;
    };

    self.getElapsedTime = () => {
      return self._elapsedTime;
    };

    self.getDateStop = () => {
      return self._dateStop;
    };

    self.getElapsedMiliSeconds = () => {
      return self._elapsedMiliSeconds;
    }

    self.reset = () => {
      self._wasStarted = false;
      self._timeStart = null;
      self._dateStart = null;

      self._wasStoped = false;
      self._dateStop = null;

      self._elapsedTime = null;
      self._elapsedMiliSeconds = null;
    };

    self.start = () => {
      let start = process.hrtime();
      let dateStart = new Date();
      self.reset();

      self._timeStart = start;
      self._dateStart = dateStart;
      self._wasStarted = true;
    };

    self.stop = () => {

      if (!self._wasStarted) {
        throw new Error('You must start the stopWatch first.');
      }

      if (!self._wasStoped) {

        self._elapsedTime = process.hrtime(self._timeStart);
        self._dateStop = new Date();

        self._wasStoped = true;
        self._elapsedMiliSeconds = (((self._elapsedTime[0] * 1e9) + self._elapsedTime[1]) * 1e-6);
      }
    };

    return self;
  }
}