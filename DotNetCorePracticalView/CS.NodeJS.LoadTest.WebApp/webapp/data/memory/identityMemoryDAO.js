module.exports = {

  _database: {},

  insert: (identity) => {

    let persisted = module.exports._database[identity.identityName];

    if (persisted) {

      throw new Error('insertion for identity "' + identity.identityName + '" fails');
    }

    module.exports._database[identity.identityName] = identity;
  },

  deleteAll: () => {

    let affected = Object.keys(module.exports._database).length;
    module.exports._database = {};
    return affected;
  }
}