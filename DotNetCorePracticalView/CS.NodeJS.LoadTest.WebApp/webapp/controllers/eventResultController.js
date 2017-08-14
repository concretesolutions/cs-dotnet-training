module.exports = {

	deleteAllFromMemory: (req, res, options) => {

		options.eventResultDAO.deleteAll();
		res.status(200);
		res.body = null;
	}
}