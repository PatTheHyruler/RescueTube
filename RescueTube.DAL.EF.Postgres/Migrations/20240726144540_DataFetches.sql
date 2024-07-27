INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, VideoId)
SELECT gen_random_uuid(), "LastSuccessfulFetchUnofficial", true, 'videopage', true, 'yt-dlp', "Id"
FROM "Videos" WHERE "LastSuccessfulFetchUnofficial" is not null;
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, VideoId)
SELECT gen_random_uuid(), "LastFetchUnofficial", false, 'videopage', true, 'yt-dlp', "Id"
FROM "Videos" WHERE "LastFetchUnofficial" is not null AND "LastFetchUnofficial" > "LastSuccessfulFetchUnofficial";
-- Video DownloadAttempts
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, VideoId)
SELECT gen_random_uuid(), "AddedToArchiveAt", false, 'videofiledownload', false, 'yt-dlp', "Id"
FROM "Videos" WHERE "FailedDownloadAttempts" > 0;
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, VideoId)
SELECT gen_random_uuid(), vf."LastFetched", true, 'videofiledownload', false, 'yt-dlp', v."Id"
FROM "Videos" v INNER JOIN "VideoFiles" vf ON vf."VideoId" = v."Id";
-- Video AuthorFetches
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, VideoId)
SELECT gen_random_uuid(), "AddedToArchiveAt", false, 'videoauthor', false, 'general', "Id"
FROM "Videos" WHERE "FailedAuthorFetches" > 0;
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, VideoId)
SELECT gen_random_uuid(), "AddedToArchiveAt", true, 'videoauthor', false, 'general', "Id"
FROM "Videos" WHERE "FailedAuthorFetches" = 0;
-- Video LastCommentsFetch
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, VideoId)
SELECT gen_random_uuid(), "LastCommentsFetch", true, 'comments', false, 'yt-dlp', "Id"
FROM "Videos" WHERE "LastCommentsFetch" IS NOT NULL;

INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, PlaylistId)
SELECT gen_random_uuid(), "LastSuccessfulFetchUnofficial", true, 'playlist', true, 'yt-dlp', "Id"
FROM "Playlists" WHERE "LastSuccessfulFetchUnofficial" is not null;
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, PlaylistId)
SELECT gen_random_uuid(), "LastFetchUnofficial", false, 'playlist', true, 'yt-dlp', "Id"
FROM "Playlists" WHERE "LastFetchUnofficial" is not null AND "LastFetchUnofficial" > "LastSuccessfulFetchUnofficial";

INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, CommentId)
SELECT gen_random_uuid(), "LastSuccessfulFetchUnofficial", true, 'comments', true, 'yt-dlp', "Id"
FROM "Comments" WHERE "LastSuccessfulFetchUnofficial" is not null;
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, CommentId)
SELECT gen_random_uuid(), "LastFetchUnofficial", false, 'comments', true, 'yt-dlp', "Id"
FROM "Comments" WHERE "LastFetchUnofficial" is not null AND "LastFetchUnofficial" > "LastSuccessfulFetchUnofficial";

-- Authors added via comments
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, AuthorId)
SELECT gen_random_uuid(), "LastSuccessfulFetchUnofficial", true, 'comments', true, 'yt-dlp', "Id"
FROM "Authors" WHERE "DisplayName" is null AND "LastSuccessfulFetchUnofficial" is not null;
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, AuthorId)
SELECT gen_random_uuid(), "LastFetchUnofficial", false, 'comments', true, 'yt-dlp', "Id"
FROM "Authors" WHERE "DisplayName" is null AND "LastFetchUnofficial" is not null AND "LastFetchUnofficial" > "LastSuccessfulFetchUnofficial";
-- Authors added via video
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, AuthorId)
SELECT gen_random_uuid(), "AddedToArchiveAt", true, 'videopage', true, 'yt-dlp', "Id"
FROM "Authors" WHERE "DisplayName" is not null;
-- Extra author data fetched via YouTubeExplode
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, AuthorId)
SELECT gen_random_uuid(), "AddedToArchiveAt", true, 'channel', true, 'ytexplode', "Id"
FROM "Authors" WHERE "FailedExtraDataFetchAttempts" = 0;
INSERT INTO "DataFetches" (Id, OccurredAt, Success, Type, ShouldAffectValidity, Source, AuthorId)
SELECT gen_random_uuid(), "AddedToArchiveAt", false, 'channel', true, 'ytexplode', "Id"
FROM "Authors" WHERE "FailedExtraDataFetchAttempts" > 0;