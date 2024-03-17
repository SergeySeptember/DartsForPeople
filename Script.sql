-- Создаём необходимые таблицы

CREATE TABLE IF NOT EXISTS person 
(
    user_id VARCHAR(255) PRIMARY KEY, 	-- Id в телеграме
    first_name VARCHAR(255) NOT NULL,	-- Имя
    second_name VARCHAR(255) NOT NULL, 	-- Фамилия
    birth_date DATE NOT null			-- Дата рождения
);

CREATE TABLE IF NOT EXISTS word_case 
(
    birthday_case VARCHAR(255),	-- Варианты поздравления с ДР
    silence_case VARCHAR(255),	-- Варианты реакций на молчание
    react_case VARCHAR(255)		-- Варианты слов, на которые нужно отреагировать
);

CREATE TABLE IF NOT EXISTS settings 
(
    token VARCHAR(255), -- Токен бота
    chat_id bigint, -- Id чата, если изсвестен. Если нет, бот сам его запишет, после первого сообщения в чате.
    silence_timer bigint, -- Часы в миллисекундах
    days text[] -- Дни, когда нужно запустить в голосование в чате.
);

-- Дале вставляем обязательные данные для работы бота

INSERT INTO settings (token, chat_id, silence_timer, days) 
VALUES 
(
'your_token' 	
, -4120049531	
, 43200000		
, '{Monday, Tuesday, Wednesday, Friday}' 
)

-- Заполняем остальные таблицы

INSERT INTO person (user_id, first_name, second_name, birth_date) 
VALUES 
('Sergey_September', 'Серёга', 'Мешков', '1998-09-19')


INSERT INTO word_case (birthday_case) 
VALUES 
('Поздравляем его с приближением к смерти!')
,('Накидайте ему поздравлений!')
,('Желаем меткой руки, острого зрения и всегда попадать точно в цель!')

INSERT INTO word_case (silence_case) 
VALUES 
('Тишина как в морге...')
,('Веселее, веселее, ребята!')

INSERT INTO word_case (react_case) 
VALUES 
('бля')
,('пизд')
,('хуй')
,('сука')

