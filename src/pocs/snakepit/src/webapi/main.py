from flask import Flask
from flask_cors import CORS
from datetime import datetime

app= Flask(__name__)
CORS(app)

@app.route("/")
def timestamp():
    return "Welcome to the snakepit, there are {} snakes here".format(datetime.now().timestamp())

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0')
