module.exports = {

  _database: [],

  insert: (eventResult) => {

    module.exports._database.push(eventResult);
  },

  deleteAll: () => {

    let affected = module.exports._database.length;
    module.exports._database = [];
    return affected;
  },

  selectAll: () => {
    
    return module.exports._database.slice();
  }
}