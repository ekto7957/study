from flask import Flask, jsonify, request
from flask_sqlalchemy import SQLAlchemy
import mysql.connector

try:
    cnx = mysql.connector.connect(user='root', password='2rw4S@d3f4',
                                  host='localhost',
                                  database='testdb')
    print("Connection Successful")
except mysql.connector.Error as err:
    if err.errno == mysql.connector.errorcode.ER_ACCESS_DENIED_ERROR:
        print("Something is wrong with your user name or password")
    elif err.errno == mysql.connector.errorcode.ER_BAD_DB_ERROR:
        print("Database does not exist")
    else:
        print(err)
else:
    cnx.close()



mydb = mysql.connector.connect(
  host="localhost",
  user="root",
  password="2rw4S@d3f4",
  database="testdb"
)

mycursor = mydb.cursor()

mycursor.execute("CREATE TABLE user (id INT AUTO_INCREMENT PRIMARY KEY, name VARCHAR(255), email VARCHAR(255), password VARCHAR(255))")



app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'mysql+pymysql://root:2rw4S@d3f4@localhost/testdb'
db = SQLAlchemy(app)

class test(db.Model):
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(80), unique=True, nullable=False)
    email = db.Column(db.String(120), unique=True, nullable=False)

    def __repr__(self):
        return '<test %r>' % self.username
    


# 새 사용자 생성하기

@app.route("/sign_up",methods=['POST'])
def sign_up():
    user_data = request.json
    new_user = test(username=user_data['name'], email=user_data['email'])
    db.session.add(new_user)
    db.session.commit()
    return jsonify({'message': 'New user created!'}), 200

