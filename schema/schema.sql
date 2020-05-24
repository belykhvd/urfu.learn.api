-------------------------------------------
create extension if not exists "uuid-ossp";
create extension if not exists "pgcrypto";
-------------------------------------------
create table if not exists auth_data
(
    email         text  primary key,
    password_hash bytea not null,
    user_id       uuid  not null
);
-------------------------------------------
create table if not exists "group"
(
	id       uuid  primary key,
	deleted  bool  not null,
	version  int   not null,
	data     jsonb not null
);

create table if not exists group_membership
(
	user_id  uuid primary key,
	group_id uuid not null
);
-------------------------------------------
create table if not exists course
(
	id       uuid  primary key,	
	version  int   not null,
	data     jsonb not null
);

create table if not exists course_index
(
	id                uuid primary key,
	name              text not null,
	short_description text
);
-------------------------------------------
create table if not exists challenge
(
	id       uuid  primary key,	
	data     jsonb not null
);

create table if not exists challenge_accomplishment
(
	user_id      uuid not null,
	challenge_id uuid not null,
	number       int  not null,
	status       bool not null
);
-------------------------------------------
create table if not exists profile
(
	id       uuid  primary key,
	deleted  bool  not null,
	version  int   not null,
	data     jsonb not null
);

create table if not exists profile_index
(
	user_id  uuid primary key,
	fullname text not null
);
-------------------------------------------