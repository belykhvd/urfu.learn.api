import requests as req


URL_PREFIX = 'http://localhost:8080/'

def route(url):
    return URL_PREFIX + url


auth_data = {
    "Email": "krivedina@gmail.com",
    "Password": "123456"
}

registration_data = {
    "Surname": "Казаков",
    "FirstName": "Михаил",
    "SecondName": "Альбертович",
    "Email": auth_data["Email"],
    "Password": auth_data["Password"],
    "Group": "МЕН-460207"
}

# observe / list


[
    ('post', 'signUp', 1),                                   # регистрируется
    ('post', 'signIn', 1),                                   # логинится
    ('get',  'profile/{userId}', 1),                         # заходит в профиль
    ('get',  'profile/{userId}/photo', 1),                   #
    ('post', 'profile/{userId}/photo/update', 1),             # апдейтит фото
    ('post', 'profile/{userId}/update', 1),                   # добавляет в профиле отчество
    ('get',  'user/{userId}/courses', 1),                     # просматривает список своих курсов (paging?)

    ('get',  'user/{userId}/courses/...', 1),                 # просматривает список обязательных в семестре курсов
    ('get',  'courses', 1),                                   # просматривает список всех курсов  (paging?)

    ('get',  'course/{courseId}', 1),                         # смотрит инфо по конкретному курсу
    ('get',  'course/{courseId}/enroll', 1),                  # записывается на курс (? на обязательные курсы он уже записан ?)

    ('get',  'course/{courseId}/tasks', 1),                   # смотрит список задач выбранного курса
    ('get',  'task/{taskId}', 1),                             # смотрит конкретную задачу
    ('get',  'user/{userId}/courses/{courseId}/tasks/{taskId}/solutions'), # ???????? получает список своих уже отправленных решений

    ('get',  '', 1),                                          # получает полную инфо по последнему решению (тесты, комментарии)

    ('post', '', 1),                                          # отправляет следующее решение

    ('signOut', 1)                                           # выходит
]


def sign_up():
    r = req.post(route('signUp'), data=registration_data)

def sign_in():
    r = req.post(rout('signIn'), data=auth_data)

#paging!
def get_my_courses():
    pass

def get_all_courses():
    pass

def get_course_info():
    pass

def course_enroll():
    pass

#paging!
def task_get_all():
    pass

def task_get():
    pass

#paging!
def solution_get_all():
    pass

def solution_add():
    pass


#def get_available_courses ?


GET = 'GET'
POST = 'POST'

def r(method, route, data = None):
    url = f'{URL_PREFIX}{route}'
    print(url)
    if method == GET:
        resp = req.get(url)
        print(f'status: {resp.status_code}\nbody: {resp.text}')
    print()




r(GET, 'courses')
