from flask import Flask, request, jsonify, make_response
from flask_sqlalchemy import SQLAlchemy
from werkzeug.security import check_password_hash

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'mysql+pymysql://root:2rw4S@d3f4@localhost/testdb'
db = SQLAlchemy(app)
print(app)
print(db)
print(db.Model)


class User(db.Model):
    ID = db.Column(db.String(255), primary_key=True)
    Password = db.Column(db.String(255), nullable=False)
    Name = db.Column(db.String(255), nullable=False)
    PhoneNumber = db.Column(db.String(20))
    Address = db.Column(db.String(255))
    Gender = db.Column(db.String(1))
    BirthDate = db.Column(db.Date)
    BusinessRegistrationNumber = db.Column(db.String(255))
    Email = db.Column(db.String(255))
    IDCreated = db.Column(db.DateTime)
    LastUpdated = db.Column(db.DateTime)
    SSN = db.Column(db.String(255))


@app.route('/users', methods=['GET'])
def get_users():
    users = User.query.all()
    return jsonify([user.to_dict() for user in users])

@app.route('/users', methods=['POST'])
def add_user():
    data = request.get_json()
    hashed_password = generate_password_hash(data['password'], method='sha256')
    new_user = User(id=data['id'], password=hashed_password)
    db.session.add(new_user)
    db.session.commit()
    return jsonify({'message': 'New user added'})

@app.route('/login', methods=['POST'])
def login():
    data = request.get_json()
    user = User.query.filter_by(id=data['id']).first()

    if not user or not check_password_hash(user.password, data['password']):
        return make_response('Invalid login information', 401)

    return jsonify({'message': 'Logged in successfully'})

if __name__ == '__main__':
    app.run(debug=True)