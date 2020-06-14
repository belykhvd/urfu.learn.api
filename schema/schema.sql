create extension if not exists "uuid-ossp";
create extension if not exists "pgcrypto";

drop table if exists auth;
drop table if exists registration;
drop table if exists user_profile;
drop table if exists user_index;
drop table if exists course;
drop table if exists course_index;
drop table if exists course_tasks;
drop table if exists task;
drop table if exists task_index;
drop table if exists task_progress;
drop table if exists "group";
drop table if exists group_index;
drop table if exists file_index;
drop table if exists attachment;
drop table if exists invite;
drop table if exists js_check_result;
drop table if exists js_queue;
drop table if exists course_access;

drop index if exists idx_group_course;

create table if not exists registration
(
	secret uuid primary key,
	email text not null,
	password_hash bytea not null,
	sent bool not null default false
);

create table if not exists auth
(
    email text primary key,
    password_hash bytea not null,
    user_id uuid not null,
    role int not null
);

create table if not exists user_profile
(
    id uuid primary key,
    data jsonb not null
);

create table if not exists user_index
(
    id uuid primary key,
    fio text not null
);

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

create table if not exists task
(
	id uuid primary key,
	data jsonb not null
);

create table if not exists task_index
(
	id uuid primary key,
	name text not null,
	max_score int not null default 0,
	requirements jsonb
);

create table if not exists task_progress
(
	user_id uuid not null,
	task_id uuid not null,
	score int not null default 0,
	done uuid[]
);

create unique index if not exists idx_user_task on task_progress (user_id, task_id);

create table if not exists "group"
(
	id uuid primary key,
	is_deleted bool not null default false,
	data jsonb not null
);

create table if not exists group_index
(
	id uuid primary key,
	is_deleted bool not null,
	name text not null
);

create table if not exists file_index
(
	id uuid primary key,
	name text not null,
	size bigint not null,
	timestamp timestamp not null,
	author uuid not null
);

create table if not exists invite
(
	secret uuid primary key,
	group_id uuid not null,
	email text not null,
	is_sent bool not null default false,
	is_accepted bool not null default false,
	student_id uuid
);

create unique index if not exists idx_group_email on invite (group_id, email);

create table if not exists attachment
(
	task_id uuid not null,
	author_id uuid not null,
	attachment_id uuid not null,
	number int not null,
	type int not null
);

create table if not exists js_test
(
	task_id uuid not null,
	name text not null,
	number int not null
);

create table if not exists js_check_result
(
	solution_id uuid primary key,
	passed int not null,
	all_count int not null,
	failed_number int,
	stacktrace text
);

create table if not exists js_queue
(	
    task_id uuid not null,
    solution_id uuid not null,
	status int not null default 0
);

create table if not exists course_access
(
	group_id uuid not null,
	course_id uuid not null
);

create index if not exists idx_group_course on course_access (group_id, course_id);

