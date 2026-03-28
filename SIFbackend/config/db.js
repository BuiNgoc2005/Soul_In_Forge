const mongoose = require("mongoose");

const connectDB = async () => {
    try {
        await mongoose.connect("mongodb+srv://buivobaongochp2005_db_user:tXwtCOac64xFSqTb@cluster0.t9bwizt.mongodb.net/SIFgame?retryWrites=true&w=majority&appName=Cluster0");
        console.log("MongoDB connected");
    } catch (err) {
        console.error(err);
        process.exit(1);
    }
};

module.exports = connectDB;
