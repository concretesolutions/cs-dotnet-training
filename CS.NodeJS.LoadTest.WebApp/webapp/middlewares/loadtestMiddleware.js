module.exports = {

  handleError: (err, req, res, next) => {

    res.status(500);
    res.body = 'Internal Server Error.';
    console.error(err);
    next();
  },

  handle404: (req, res, next) => {
    res.status(404)
    res.body = 'Not Found !'
    next();
  },

  sendResponse: (req, res) => {
    if (res.body) {
      res.send(res.body);
    }
    else {
      res.end();
    }
  }
}