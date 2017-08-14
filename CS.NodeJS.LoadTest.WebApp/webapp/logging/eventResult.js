let eventModule = require('./event.js');

module.exports = {

  create: (event, eventStart, elapsedMiliSeconds, statusCode, testId, language) => {

    return {
      event: event,
      eventStart: eventStart,
      elapsedMiliSeconds: elapsedMiliSeconds,
      statusCode: statusCode,
      test: {
        testId: testId,
        language: language
      }
    };
  },

  createEventResultCluster: (eventResultClusterMetrics, clusterDateTime) => {

    return {
      metrics: eventResultClusterMetrics,
      clusterDateTime: clusterDateTime
    };
  },

  createEventResultClusterMetrics: (eventResultArray) => {

    let eventClusterMetrics = {
      successCount: 0,
      errorCount: 0,
      elapsedTimeAvg: 0.0
    };

    eventResultArray.forEach((ev) => {

      if (ev.statusCode == eventModule.getExpectedStatusCode(ev.event.eventId)) {
        eventClusterMetrics.successCount++;
      }
      else {
        eventClusterMetrics.errorCount++;
      }

      eventClusterMetrics.elapsedTimeAvg += ev.elapsedMiliSeconds;
    });

    eventClusterMetrics.elapsedTimeAvg = eventClusterMetrics.elapsedTimeAvg /
      (eventClusterMetrics.successCount + eventClusterMetrics.errorCount);

    return eventClusterMetrics;
  }
};