let event = require('../logging/event.js');
let eventResult = require('../logging/eventResult.js');
let stopwatch = require('../util/stopwatch.js');

module.exports = {

  initEventContext: (req, res, next) => {

    req.eventContext = {
      event: null,
      stopWatch: stopwatch.create()
    };

    req.eventContext.stopWatch.start();
    next();
  },

  _saveEventResult: (req, res, options) => {

    //recording eventResult
    let testId = req.headers['x-loadtest-id'] || null;
    let language = req.headers['x-loadtest-lg'] || null;

    let objEventResult = eventResult.create(
      req.eventContext.event,
      req.eventContext.stopWatch.getDateStart(),
      req.eventContext.stopWatch.getElapsedMiliSeconds(),
      res.statusCode,
      testId,
      language
    );

    options.eventResultDAO.insert(objEventResult);
  },

  create: (options) => {

    let self = {
      _options: options
    };

    self.saveEventResult = (req, res, next) => {

      req.eventContext.stopWatch.stop();

      if (req.eventContext.event) {
        module.exports._saveEventResult(req, res, self._options);
      }

      next();
    };

    return self;
  }
};