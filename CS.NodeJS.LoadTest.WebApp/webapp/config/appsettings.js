let configExtend = require('config-extend');
let secrets = require('../secrets/secrets.js');
let loadTestDBMS = require('../data/loadTestDBMS.js');

let appSettings = {

  encoding: 'utf8',
  DBMS: loadTestDBMS.mongodb,

  databaseConfig: {
    pgsql: {
      collate: 'English_United States.1252',
      ctype: 'English_United States.1252'
    },
    mysql: {
      collate: 'utf8_general_ci',
      ctype: 'utf8'
    },
    mongodb: {
      collate: '',
      ctype: ''
    }
  }
}

if (process.env.NODE_ENV) {

  let envSettingsPath = './appsettings.' + process.env.NODE_ENV + '.js';

  try {
    let envSettings = require(envSettingsPath);
    appSettings = configExtend(appSettings, envSettings);
  }
  catch (err) {
    console.warn('error loading settings: ' + envSettingsPath);
  }
}

appSettings = configExtend(appSettings, secrets);
module.exports = appSettings;