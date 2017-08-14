let eventResultModule = require('./eventResult.js');

module.exports = {

	_distinct: (acc, val) => {

		acc = Array.isArray(acc) ? acc : [];

		if (!acc.includes(val)) {
			acc.push(val);
		}

		return acc;
	},

	_clusterizeEvents: (eventResultArray) => {

		let eventSuperCluster = [];

		eventResultArray.forEach((e) => {

			let clusterDateTime = new Date(e.eventStart);
			clusterDateTime.setMilliseconds(0);

			let clusterIndex = eventSuperCluster.findIndex((cl) => {

				return cl.clusterDatetTime.getTime() == clusterDateTime.getTime();
			});

			if (clusterIndex > -1) {

				eventSuperCluster[clusterIndex].eventResultArray.push(e);
			}
			else {

				eventSuperCluster.push({
					clusterDatetTime: clusterDateTime,
					eventResultArray: [e]
				});
			}
		});

		return eventSuperCluster;
	},

	compileTestResult: (eventResultArray) => {

		let testResultArray = [];
		let testEvents = eventResultArray.filter((e) => { return e.test && e.test.testId; });

		if (testEvents.length < 1) {
			return testResultArray;
		}

		let testIdArray = testEvents
			.map((t) => { return t.test.testId; })
			.reduce(module.exports._distinct);

		let languageArray = testEvents
			.map((e) => { return e.test.language; })
			.reduce(module.exports._distinct);

		testIdArray.forEach((testId) => {
			languageArray.forEach((language) => {

				let testLanguageEvents = testEvents.filter((e) => {
					return e.test.testId == testId && e.test.language == language;
				});

				let testResult = {
					test: {
						testId: testId,
						language: language
					},

					metrics: eventResultModule.createEventResultClusterMetrics(testLanguageEvents),
					superCluster: []
				};

				let eventSuperCluster = module.exports._clusterizeEvents(testLanguageEvents);

				eventSuperCluster.forEach((cluster) => {

					let eventResultClusterMetrics = eventResultModule
						.createEventResultClusterMetrics(cluster.eventResultArray);

					testResult.superCluster.push(eventResultModule
						.createEventResultCluster(eventResultClusterMetrics, cluster.clusterDatetTime));
				});

				testResultArray.push(testResult);
			});
		});

		return testResultArray;
	}
}