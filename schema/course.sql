create table if not exists course
(
	id uuid primary key,
	data jsonb not null
);

create table if not exists course_index
(
	id uuid primary key,
	name text not null,
	max_score int not null default 0
);

create table if not exists course_tasks
(
	course_id uuid not null,
	task_id uuid not null
);